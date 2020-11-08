using System.Web;
using System.Web.Mvc;

namespace LAB_7_CHAT_WEBSOCKETS_SERVER_ASP_MVC
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
