using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;

namespace Teste
{
    public class MockControllerBuilder<T> where T : ControllerBase
    {
        private String httpMethodType = "Get";
        private String authUserName = String.Empty;
        private Boolean isAjaxRequest = false;

        private HttpRequestBase mockHttpRequestBase;
        private HttpSessionStateBase mockHttpSessionStateBase;

        public MockControllerBuilder<T> Method(String methodType)
        {
            if (String.IsNullOrEmpty(methodType))
                throw new ArgumentNullException("methodType");

            this.httpMethodType = methodType;
            return this;
        }

        public MockControllerBuilder<T> SetAuthUser(String userName) 
        {
            if (String.IsNullOrEmpty(userName))
                throw new ArgumentNullException("userName");

            this.authUserName = userName;
            return this;
        }

        public MockControllerBuilder<T> IsAjax() 
        {
            this.isAjaxRequest = true;
            return this;
        }

        public MockControllerBuilder<T> HttpRequestBase(HttpRequestBase mock)
        {
            this.mockHttpRequestBase = mock;
            return this;
        }

        public MockControllerBuilder<T> HttpSessionStateBase(HttpSessionStateBase mock)
        {
            this.mockHttpSessionStateBase = mock;
            return this;
        }

        protected NameValueCollection CreateHeader()
        {
            var result = new NameValueCollection();

            // Add header value for ajax requests
            if (this.isAjaxRequest)
                result.Add("X-Requested-With", "XMLHttpRequest");

            return result;
        }

        protected Mock<HttpRequestBase> DefaultRequestBase() 
        {
            var result = new Mock<HttpRequestBase>();

            result.Setup(r => r.HttpMethod).Returns(this.httpMethodType);
            result.Setup(r => r.Headers).Returns(this.CreateHeader());
            result.Setup(r => r.Form).Returns(new NameValueCollection());
            result.Setup(r => r.QueryString).Returns(new NameValueCollection());

            /*
            if (!String.IsNullOrEmpty(this.authUserName))
            {
                //((System.Security.Claims.ClaimsIdentity)(((((System.Web.Mvc.Controller)(this)).HttpContext).Request).LogonUserIdentity))
                
                //sUserPrincipalName
                //WindowsIdentity.GetCurrent()
                
                var identity = new Mock<WindowsIdentity>("guilhermevideira@guilhermito*86.com");  //WindowsIdentity.GetCurrent();
                identity.SetupGet(i => i.Name).Returns(this.authUserName);
                result.SetupGet(r => r.LogonUserIdentity).Returns(identity.Object);
            }
            */

            return result;
        }

        protected Mock<HttpSessionStateBase> DefaultSessionStateBase()
        {
            return new Mock<HttpSessionStateBase>();
        }

        public T Mock()
        {
            var httpRequestBase = this.mockHttpRequestBase ?? DefaultRequestBase().Object;
            var httpSessionStateBase = this.mockHttpSessionStateBase ?? DefaultSessionStateBase().Object;


            var writer = Console.Out; // new StringWriter();
            var httpResponse = new HttpResponse(writer);
            var httpResponseBase = new HttpResponseWrapper(httpResponse);


            //var httpServerUtility = new HttpServerUtility();
            //var httpServerUtilityWrapper = new HttpServerUtilityWrapper(httpServerUtility);


            var mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(c => c.Request).Returns(httpRequestBase);
            mockHttpContext.SetupGet(c => c.Session).Returns(httpSessionStateBase);
            mockHttpContext.SetupGet(c => c.Response).Returns(httpResponseBase);
            //mockHttpContext.SetupGet(c => c.Server).Returns(httpServerUtilityWrapper);


            var controllerContext = new ControllerContext(mockHttpContext.Object, new RouteData(), new Mock<ControllerBase>().Object);

            var result = Activator.CreateInstance<T>();
            result.ControllerContext = controllerContext;

            return result;
        }
    }
}