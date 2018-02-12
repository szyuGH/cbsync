using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace CBSync.Controllers
{
    public class SyncController : ApiController
    {
        public HttpResponseMessage GetPing()
        {
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        }
    }
}
