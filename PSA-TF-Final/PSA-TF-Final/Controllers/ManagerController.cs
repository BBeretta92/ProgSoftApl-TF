using PSA_TF_Final.App_Start;
using PSA_TF_Final.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PSA_TF_Final.Controllers
{
    public class ManagerController : Controller
    {
        #region Meses
        private List<Object> months = new List<Object>
            {
                new { text = "Janeiro", value = "1" },
                new { text = "Fevereiro", value = "2" },
                new { text = "Março", value = "3" },
                new { text = "Abril", value = "4" },
                new { text = "Maio", value = "5" },
                new { text = "Junho", value = "6" },
                new { text = "Julho", value = "7" },
                new { text = "Agosto", value = "8" },
                new { text = "Setembro", value = "9" },
                new { text = "Outubro", value = "10" },
                new { text = "Novembro", value = "11" },
                new { text = "Dezembro", value = "12" }
           };
        #endregion

        /// <summary>
        ///     Este método redireciona para a tela principal da Manager (Index)
        /// </summary>
        /// <returns>Tela Index</returns>
        [Authorize]
        public ActionResult Index()
        {
            GetRole();
            ViewBag.Relatorio = null;
            return View();
        }

        /// <summary>
        ///     Método utilizado para realizar o pagamento de um ticket. O ticket é recuperado da base de dados,
        ///     é verificado a data em que o ticket foi emitido e então o valor do mesmo é gerado. Atualiza o 
        ///     valor na base de dados e retorna ao usuario.
        /// </summary>
        /// <returns>Tela Index</returns>
        public ActionResult PayTicket(DAOTicket ticket)
        {
            var dateNow = DateTime.Now;
            DAOTicket dbTicket = new DAOTicket();
            using (var db = new DataBaseContext())
            {
                dbTicket = (from DAOTicket t in db.Tickets
                            where t.Codigo.Equals(ticket.Codigo)
                            select t).SingleOrDefault<DAOTicket>();

                if (dbTicket != null)
                {
                    if (!String.IsNullOrEmpty(dbTicket.MotivoSaida))
                        ViewBag.Error = "Ticket já liberado. Motivo : " + dbTicket.MotivoSaida;
                    else if (!dbTicket.Pago)
                    {
                        var difference = dateNow - dbTicket.Entrada;

                        if (dbTicket.Codigo.EndsWith("G") || difference.Days > 0)
                            dbTicket.Valor = 50;
                        else if (difference.Hours == 0 && difference.Minutes <= 15)
                            dbTicket.Valor = 0;
                        else if (difference.Hours <= 3)
                            dbTicket.Valor = 5;
                        else if (difference.Hours > 3 && difference.Hours < 15)
                            dbTicket.Valor = 20;
                        else if (difference.Hours >= 15)
                            dbTicket.Valor = 50;

                        dbTicket.Pago = true;
                        db.SaveChanges();
                        ViewBag.ShowPrice = true;
                        ViewBag.Error = "Ticket pago, muito obrigado!";
                    }
                    else
                        ViewBag.Error = "Ticket ja foi pago.";
                }
                else
                {
                    ViewBag.Error = "Código inválido, por favor tente novamente.";
                }
            }
            ViewBag.Funcao = "Pagar Ticket";
            return View("Index", dbTicket);
        }

        /// <summary>
        ///     Método utilizado para mostrar os campos necessários para inserção do código do ticket
        ///     e pagamento do mesmo.
        /// </summary>
        /// <returns>Tela Index</returns>
        public ActionResult PayTicketFields()
        {
            ViewBag.Funcao = "Pagar Ticket";
            return View("Index");
        }

        /// <summary>
        ///     Método utilizado para mostrar os campos necessários para inserção do código do ticket
        ///     e consulta do preço do mesmo.
        /// </summary>
        /// <returns>Tela Index</returns>
        public ActionResult ShowPrice()
        {
            ViewBag.Fields = true;

            return View("Index");
        }

        /// <summary>
        ///     Método utilizado para verificar qual o Role do usuário atual (admin, operador de caixa ou gerente)
        /// </summary>
        /// <returns>String com o role do usuario ou vazio se não houver.</returns>
        private string checkUserRole()
        {

            if (User.Identity.IsAuthenticated)
            {
                var user = User.Identity;
                ApplicationDbContext context = new ApplicationDbContext();
                var roles = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

                var role = roles.GetRoles(user.GetUserId());

                if (role.Count > 0)
                    return role[0].ToString();
                else
                    return String.Empty;
            }
            return String.Empty;

        }

        /// <summary>
        ///     Método utilizado para mostrar os campos necessários para inserção do código do ticket
        ///     e liberação do mesmo sem pagamento.
        /// </summary>
        /// <returns>Tela Index</returns>
        public ActionResult ShowReleaseTicketFields()
        {
            ViewBag.Funcao = "Liberar Ticket";
            return View("Index");
        }

        /// <summary>
        ///     Método utilizado para armazenar a razão pela qual um ticket foi leberado sem pagamento.
        /// </summary>
        /// <param name="codigo">Código do ticket liberado</param>
        /// <param name="reason">Razão pela qual o ticket foi liberado</param>
        /// <returns></returns>
        public ActionResult ReleaseTicket(string codigo, string reason)
        {
            if (!String.IsNullOrEmpty(reason))
            {
                DAOTicket dbTicket = new DAOTicket();
                using (var db = new DataBaseContext())
                {
                    dbTicket = (from DAOTicket t in db.Tickets
                                where t.Codigo.Equals(codigo)
                                select t).SingleOrDefault<DAOTicket>();
                    if (dbTicket == null)
                        ViewBag.Status = "Código do ticket inválido, favor tente novamente.";
                    else
                    {
                        ViewBag.Status = "Ticket liberado com sucesso!";
                        if (String.IsNullOrEmpty(dbTicket.MotivoSaida) && !dbTicket.Pago)
                            dbTicket.MotivoSaida = reason;
                        db.SaveChanges();
                    }
                }
                ViewBag.Funcao = "Liberar Ticket";
            }
            else
            {
                ViewBag.Status = "Por favor, selecione um motivo para a liberação.";
            }
            ViewBag.Funcao = "Liberar Ticket";
            return View("Index");
        }

        /// <summary>
        ///     Método utilizado para gerar um novo ticket no guichê. Neste método é adicionado a letra "G" ao
        ///     código do ticket, o que significa que este ticket foi gerado no guichê. Também é adicionado 
        ///     um motivo "Extravio" no ticket, para mostrar o motivo pelo qual aquele ticket foi gerado no 
        ///     guichê.
        /// </summary>
        /// <returns></returns>
        public ActionResult GenerateTicket()
        {
            DAOTicket newTicket = new DAOTicket()
            {
                Codigo = GenerateCode() + "G",
                Entrada = DateTime.Now,
                MotivoSaida = "Extravio"
            };
            using (var db = new DataBaseContext())
            {
                var dbTicket = (from ticket in db.Tickets
                                where ticket.Codigo.Equals(newTicket.Codigo)
                                select ticket).Count();

                if (dbTicket <= 0)
                {
                    db.Tickets.Add(newTicket);
                    db.SaveChanges();
                }
            }
            ViewBag.Funcao = "Gerar Ticket";
            return View("Index", newTicket);
        }

        /// <summary>
        ///     Método invocado pelo GenerateTicket. Este método gera um novo codigo para um ticket.
        /// </summary>
        /// <returns></returns>
        private string GenerateCode()
        {
            Random randNum = new Random();
            return randNum.Next(999999).ToString();
        }

        /// <summary>
        ///     Método que redireciona o usuário para a tela de Reports, onde é possível ver os relatórios.
        ///     Esse método é ativado ao pressionar o botão de relatóriso na tela do administrador, apenas o 
        ///     gerente deve poder apertar este botão.
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ActionResult Reports()
        {
            ViewBag.months = months;
            GetRole();
            return View("Reports");
        }

        /// <summary>
        ///     Método utilizado para gerar um relatório com todos os tickets.
        /// </summary>
        /// <returns></returns>
        public ActionResult PaymentReport()
        {
            List<DAOTicket> tickets = GetPayments();

            ViewBag.Total = tickets.Sum(t => t.Valor);
            ViewBag.Relatorio = "Pagamentos";
            ViewBag.months = months;
            ViewBag.months = months;
            GetRole();
            return View("Reports", tickets);

        }

        /// <summary>
        ///     Método utilizado para gerar o relatório com os tickets pagos.
        /// </summary>
        /// <returns></returns>
        public ActionResult TotalReport()
        {
            IList<DAOTicket> tickets = GetTotalPayments();

            ViewBag.Total = tickets.Count();
            ViewBag.Relatorio = "Pagos";
            ViewBag.months = months;
            ViewBag.months = months;
            GetRole();
            return View("Reports", tickets);

        }

        /// <summary>
        ///     Método utilizado para retornar a lista de tickets liberados sem pagamento para a view.
        ///     Este método é executado quando a opção para ver o relatório de tickets liberados sem
        ///     pagamentos é clicado.
        /// </summary>
        /// <returns>Lista de tickets</returns>
        public ActionResult ReleasedTickets()
        {
            IList<DAOTicket> tickets = GetRelleasedTickets();

            ViewBag.Total = tickets.Count();
            ViewBag.Relatorio = "Liberados";
            ViewBag.months = months;
            GetRole();
            return View("Reports", tickets);

        }

        /// <summary>
        ///     Método auxiliar utilizado para retornar a lista de todos os tickets.
        /// </summary>
        /// <returns>Lista de tickets</returns>
        private List<DAOTicket> GetPayments()
        {
            List<DAOTicket> tickets = new List<DAOTicket>();
            using (var db = new DataBaseContext())
            {
                tickets = (from DAOTicket t in db.Tickets
                           select t).ToList();
            }
            return tickets;
        }

        /// <summary>
        ///     Método auxiliar utilizado para retornar a lista de tickets pagos.
        /// </summary>
        /// <returns>Lista de tickets</returns>
        private List<DAOTicket> GetTotalPayments()
        {
            List<DAOTicket> tickets = new List<DAOTicket>();
            using (var db = new DataBaseContext())
            {
                tickets = (from DAOTicket t in db.Tickets
                           where t.Pago
                           select t).ToList();
            }
            return tickets;
        }

        /// <summary>
        ///     Método auxiliar utilizado para retornar a lista de tickets liberados sem pagamento.
        /// </summary>
        /// <returns>Lista de tickets</returns>
        private List<DAOTicket> GetRelleasedTickets()
        {
            List<DAOTicket> tickets = new List<DAOTicket>();
            using (var db = new DataBaseContext())
            {
                tickets = (from DAOTicket t in db.Tickets
                           where !String.IsNullOrEmpty(t.MotivoSaida)
                           select t).ToList();
            }
            return tickets;
        }

        /// <summary>
        ///     Método executado quando um filtro por data é realizado. Retorna a lista selecionada
        ///     filtrada pela data inserida.
        /// </summary>
        /// <param name="tipoRelatorio">Tipo de relatório que está sendo filtrado.</param>
        /// <param name="data">Data usada para realizar o filtro.</param>
        /// <returns></returns>
        public ActionResult FilterList(string tipoRelatorio, string dia, string mes)
        {
            ViewBag.months = months;
            GetRole();
            if (String.IsNullOrEmpty(tipoRelatorio))
            {
                ViewBag.Error = "Por favor, selecione um tipo de relatório";
            }
            else
            {
                IList<DAOTicket> tickets = new List<DAOTicket>();
                mes = mes.Length == 1 ? "0" + mes : mes;
                string data = dia + "/" + mes;
                switch (tipoRelatorio)
                {
                    case "Pagamentos":
                        tickets = GetPayments().Where(t => t.Entrada.ToShortDateString().Contains(data)).ToList();
                        ViewBag.Total = tickets.Sum(t => t.Valor);
                        ViewBag.Relatorio = "Pagamentos";
                        break;
                    case "Pagos":
                        tickets = GetTotalPayments().Where(t => t.Entrada.ToShortDateString().Contains(data)).ToList();
                        ViewBag.Relatorio = "Pagos";
                        ViewBag.Total = tickets.Count();
                        break;
                    case "Liberados":
                        tickets = GetRelleasedTickets().Where(t => t.Entrada.ToShortDateString().Contains(data)).ToList();
                        ViewBag.Relatorio = "Liberados";
                        ViewBag.Total = tickets.Count();
                        break;
                }

                return View("Reports", tickets);
            }

            return View("Reports");
        }

        /// <summary>
        ///     Método utilizado para verificar qual o Role do usuario atual.
        /// </summary>
        private void GetRole()
        {
            var role = checkUserRole();
            if (!String.IsNullOrEmpty(role))
                ViewBag.Role = role.ToString();
        }

        public ActionResult DeleteTickets()
        {
            using (var db = new DataBaseContext())
            {
                db.Tickets.RemoveRange(db.Tickets.ToList());

                db.SaveChanges();
            }
            ViewBag.RemoveTickets = "Os tickets foram todos apagados.";
            ViewBag.months = months;
            return View("Reports");
        }

        public ActionResult ResetVacancies()
        {
            List<DAOTicket> tickets = new List<DAOTicket>();
            using (var db = new DataBaseContext())
            {
                tickets = (from DAOTicket t in db.Tickets
                           where !t.Pago
                           select t).ToList();
                foreach (DAOTicket ticket in tickets)
                {
                    if (String.IsNullOrEmpty(ticket.MotivoSaida))
                    {
                        ticket.MotivoSaida = "Vagas Resetadas";
                    }
                    ticket.Pago = true;
                }
                db.SaveChanges();
            }

            ViewBag.ReseteVacancies = "Todos os tickets estão com o status PAGO e o motivo VAGAS RESETADAS.";
            ViewBag.months = months;
            return View("Reports");
        }


    }
}