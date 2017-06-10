using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PSA_TF_Final.Models
{
    public class DAOTicket
    {
        public int id { get; set; }

        /// <summary>
        ///     Propriedade do ticket, armazena o valor do tícket.
        /// </summary>
        [Display(Name = "Valor: ")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public double Valor { get; set; }

        /// <summary>
        ///     Propriedade do ticket, armazena o códgo do tícket.
        /// </summary>
        [Display(Name = "Código: ")]
        public string Codigo { get; set; }

        /// <summary>
        ///     Propriedade do ticket, armazena a data e horário em que o ticket foi gerado.
        /// </summary>
        [Display(Name = "Entrada: ")]
        public DateTime Entrada { get; set; }

        /// <summary>
        ///     Propriedade do ticket, armazena um booleano indicado se o ticket foi pago ou não
        /// </summary>
        [Display(Name = "Pago: ")]
        public Boolean Pago { get; set; }

        /// <summary>
        ///     Propriedade do ticket, armazena o motivo da saída sem pagamento deste tícket.
        /// </summary>
        public string MotivoSaida { get; set; }
    }
}