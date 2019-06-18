using DynamicForms.Providers.DynamicForms.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace DynamicForms.Providers.DynamicForms
{
    public class DynamicFormsRazorViewEngine : RazorViewEngine, IRazorViewEngine, IViewEngine
    {
        public DynamicFormsRazorViewEngine(IRazorPageFactoryProvider pageFactory, IRazorPageActivator pageActivator, HtmlEncoder htmlEncoder, IOptions<RazorViewEngineOptions> optionsAccessor, RazorProject razorProject, ILoggerFactory loggerFactory, DiagnosticSource diagnosticSource)
        : base(pageFactory, pageActivator, htmlEncoder, optionsAccessor, razorProject, loggerFactory, diagnosticSource)
        {

        }

        public new  RazorPageResult FindPage(ActionContext context, string pageName)
        {
            return base.FindPage(context, pageName);
        }

        public new RazorPageResult GetPage(string executingFilePath, string pagePath)
        {
            return base.GetPage(executingFilePath, pagePath);
        }

        public new ViewEngineResult FindView(ActionContext context, string viewName, bool isMainPage)
        {
            var result = base.FindView(context, viewName, isMainPage);
            return result.Success ? ViewEngineResult.Found(result.ViewName, new DynamicFormsView(result.View)) : result;
        }

        public new ViewEngineResult GetView(string executingFilePath, string viewPath, bool isMainPage)
        {
            var result = base.GetView(executingFilePath, viewPath, isMainPage);
            return result.Success ? ViewEngineResult.Found(result.ViewName, new DynamicFormsView(result.View)) : result;
        }
    }

    public class DynamicFormsView : IView
    {
        private readonly IView _razorView;

        public DynamicFormsView(IView razorView)
        {
            _razorView = razorView;
        }

        public string Path => _razorView.Path;

        public Task RenderAsync(ViewContext context)
        {
            if (context.ViewData.ModelExplorer.Metadata is DynamicFormsModelMetadata modelMetadata)
            {
                modelMetadata.Contextualize(context.ViewData.ModelExplorer.Model);
                context.ViewData.ModelExplorer.Contextualize(context.ViewData.ModelExplorer.Metadata);
            }

            return _razorView.RenderAsync(context);
        }
    }
}
