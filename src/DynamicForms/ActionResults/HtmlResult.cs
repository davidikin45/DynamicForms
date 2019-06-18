using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace DynamicForms.ActionResults
{
    public class HtmlResult : ContentResult
    {
        public HtmlResult(string html)
        {
            Content = html;
            ContentType = "text/html";
            StatusCode = (int)HttpStatusCode.OK;
        }
    }
}
