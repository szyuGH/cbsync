using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace CBSync
{
    public class TestController : ApiController
    {
        [HttpPost]
        public string GetTest()
        {
            return "Hello World";
        }
    }
}
