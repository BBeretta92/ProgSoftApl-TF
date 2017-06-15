using PSA_TF_Final.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PSA_TF_Final.Controllers
{
    public class TicketAPIController : ApiController
    {
        private IList<DAOTicket> GetAllTicketsFromDataBase()
        {
            using (var db = new DataBaseContext())
            {
                return (from DAOTicket t in db.Tickets
                        select t).ToList();
            }
        }

        // GET: api/TicketAPI
        [AcceptVerbs("GET")]
        public IEnumerable<DAOTicket> Get()
        {
            return GetAllTicketsFromDataBase();
        }

        // GET: api/TicketAPI/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/TicketAPI
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/TicketAPI/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/TicketAPI/5
        public void Delete(int id)
        {
        }

        [AcceptVerbs("GET")]
        public IHttpActionResult GetTicket(string code)
        {
            var tickets = GetAllTicketsFromDataBase();
            DAOTicket ticket = new DAOTicket();
            if (tickets != null && tickets.Count > 0)
            {
                ticket = tickets.First(t => t.Codigo.Equals(code, StringComparison.InvariantCultureIgnoreCase));
            }
            if (ticket != null)
                return Ok(ticket);
            else
                return NotFound();
        }
    }
}