using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PSA_TF_Final.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        ///     Este método redireciona para a tela principal da home (Index)
        /// </summary>
        /// <returns>Tela Index</returns>
        public ActionResult Index()
        {
            GetRole();
            vacanciesNumber();
            return View();
        }

        /// <summary>
        ///     Método para gerar um novo ticket. Chama o método GEnerateCode para gerar um código novo e valida
        ///     se este código ja existe no banco. 
        /// </summary>
        /// <returns>Tela Index</returns>
        public ActionResult GenerateTicket()
        {
            DAOTicket newTicket = new DAOTicket()
            {
                Codigo = GenerateCode(),
                Entrada = DateTime.Now
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
            GetRole();
            vacanciesNumber();
            ViewBag.TicketReady = true;
            return View("Index", newTicket);
        }

        /// <summary>
        ///     Método executado quando o botão de saída da cancela for pressionado. Busca no banco o ticket
        ///     referente ao código inserido, verifica se o mesmo foi pago e libera ou não a cancela.
        /// </summary>
        /// <param name="ticket"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Leave(DAOTicket ticket)
        {
            using (var db = new DataBaseContext())
            {
                var dbTicket = (from DAOTicket t in db.Tickets
                                where t.Codigo.Equals(ticket.Codigo)
                                select t).SingleOrDefault<DAOTicket>();

                if (dbTicket != null)
                {
                    if (dbTicket.Pago || !String.IsNullOrEmpty(dbTicket.MotivoSaida))
                        ViewBag.Result = "Pago";
                    else if (!dbTicket.Pago)
                        ViewBag.Result = "Não pago";
                }
                else
                {
                    ViewBag.Result = "Ticket invalido";
                }
                vacanciesNumber();
                ViewBag.Fields = true;
                return View("Index");
            }
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
        ///     Metodo que calcula o número de vagas disponíveis. Utiliza para contabilizar os tickets que NÃO
        ///     foram pagos. Tickets liberados sem pagamentos são considerados como vagas ocupadas..
        /// </summary>
        public void vacanciesNumber()
        {
            var vagasTotais = 40;
            using (var db = new DataBaseContext())
            {
                var vagasOcupadas = (from t in db.Tickets
                                     where t.Pago == false
                                     select t).Count(); ;
                ViewBag.Vagas = vagasTotais - vagasOcupadas;
            }
        }

        /// <summary>
        ///     Método para mostrar os campos para inserção do código do ticket para liberar a cancela de saida.
        /// </summary>
        /// <returns></returns>
        public ActionResult ShowFields()
        {
            CleanViewBag();
            GetRole();
            ViewBag.Fields = true;
            return View("Index");
        }

        /// <summary>
        ///     Método para mostrar os campos para inserção do código do ticket para realizar uma consulta do ticket.
        /// </summary>
        /// <returns></returns>
        public ActionResult ShowPriceFields()
        {
            CleanViewBag();
            GetRole();
            ViewBag.ShowPriceFields = true;
            return View("Index");
        }


        /// <summary>
        ///     Método acionado pelo botão de consulta do preço após o código do ticket ter sido inserido.
        ///     Este método recupera o ticket do banco e valida e o ticket foi gerado no guiche, caso não tenha
        ///     sido ele verifica o horário de entrada para calcular o valor atual do ticket.
        /// </summary>
        /// <param name="ticket"></param>
        /// <returns></returns>
        public ActionResult ConsultPrice(DAOTicket ticket)
        {
            CleanViewBag();

            var dateNow = DateTime.Now;
            DAOTicket dbTicket = new DAOTicket();
            using (var db = new DataBaseContext())
            {
                dbTicket = (from DAOTicket t in db.Tickets
                            where t.Codigo.Equals(ticket.Codigo)
                            select t).SingleOrDefault<DAOTicket>();
            }

            if (dbTicket != null)
            {
                if (!dbTicket.Pago)
                {
                    var difference = dateNow - dbTicket.Entrada;
                    if (dbTicket.Codigo.EndsWith("G")) //Valida se o ticket foi impresso no guiche, nesse caso o ticket foi extraviado
                        dbTicket.Valor = 50;
                    else if (difference.Hours >= 18 || difference.Days > 0)
                        dbTicket.Valor = 50;
                    else if (difference.Hours == 0 && difference.Minutes <= 15)
                        dbTicket.Valor = 0;
                    else if (difference.Hours <= 3)
                        dbTicket.Valor = 5;
                    else if (difference.Hours > 3 && difference.Hours < 15)
                        dbTicket.Valor = 20;


                    ViewBag.ConsultPrice = true;
                    return View("Index", dbTicket);
                }
                else
                    ViewBag.Error = "Ticket ja foi pago.";
            }
            else
            {
                ViewBag.Error = "Código inválido, por favor tente novamente.";
            }
            vacanciesNumber();
            GetRole();
            return View("Index");
        }

        /// <summary>
        ///     Método utilizado para mostrar os campos para inserção do código e liberação da cancela para 
        ///     todos os tickets.
        /// </summary>
        /// <returns></returns>
        public ActionResult ReleaseGate()
        {
            CleanViewBag();
            ViewBag.ReleaseGate = true;
            GetRole();
            return View("Index");
        }

        /// <summary>
        ///     Método que armazena a razão da liberação da cancela para todos os tickets liberados.
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public ActionResult Release(string reason)
        {
            using (var db = new DataBaseContext())
            {
                var tickets = db.Tickets.ToList();
                foreach (var t in tickets)
                {
                    t.MotivoSaida = reason;
                }
                db.SaveChanges();
                ViewBag.statusCancela = "Cancela liberada!";
            }
            GetRole();
            return View("Index");
        }

        /// <summary>
        ///     Metodo utilizado para limpar a ViewBag.
        /// </summary>
        public void CleanViewBag()
        {
            ViewBag.ReleaseGate = null;
            ViewBag.ConsultPrice = null;
            ViewBag.Error = null;
            ViewBag.Fields = null;
            ViewBag.statusCancela = null;
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
    }
}