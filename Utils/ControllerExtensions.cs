using Dev_Blog.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dev_Blog.Utils
{
    public static class ControllerExtensions
    {
        public static async Task<string> RenderEmailViewAsync<TModel>(this Controller controller, string viewName, TModel model, string token, bool partial = false)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                viewName = controller.ControllerContext.ActionDescriptor.ActionName;
            }

            controller.ViewData.Model = model;
            controller.ViewData["token"] = token;

            using var writer = new StringWriter();
            IViewEngine viewEngine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
            ViewEngineResult viewResult = viewEngine.GetView("", viewName, !partial);

            if (viewResult.Success == false)
            {
                return $"A view with the name {viewName} could not be found";
            }

            ViewContext viewContext = new ViewContext(
                controller.ControllerContext,
                viewResult.View,
                controller.ViewData,
                controller.TempData,
                writer,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);

            var bodyBuilder = writer.GetStringBuilder();

            bodyBuilder = bodyBuilder.Replace("href=\"", $"href=\"{Constants.PublicUrl}");
            bodyBuilder = bodyBuilder.Replace("src=\"", $"src=\"{Constants.PublicUrl}/");
            bodyBuilder = bodyBuilder.Replace("\\", "/");

            return bodyBuilder.ToString();
        }

        /// <summary>
        /// Awful solution which allows requests to render an email without having an ongoing HttpContext by creating a request to Localhost
        /// </summary>
        /// <param name="serviceScope"></param>
        /// <param name="path">Path to the render email, should start with a forward slash</param>
        /// <param name="properties">Properties to include in the URL. Do not use sensitive information here.</param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<string> RenderEmailViewAsync(IServiceScope serviceScope, string path, Dictionary<string, string> properties, string token)
        {
            string query = "?";
            query += $"token={token}";
            foreach (var property in properties)
            {
                query += $"&{property.Key}={property.Value}";
            }

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, Constants.LocalUrl + path + query);

            var httpClient = serviceScope.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient();

            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                return "error";
            }
        }
    }
}