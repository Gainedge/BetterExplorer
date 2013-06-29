using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Threading;

using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth;
using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Context;
using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Impl;
using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Token;
using AppLimit.CloudComputing.SharpBox.Common.Net.Web;
using AppLimit.CloudComputing.SharpBox.Common.Net.Web.Http;

#if SILVERLIGHT
using AppLimit.CloudComputing.oAuth.WP7.SilverLightHelper;
#endif

namespace AppLimit.CloudComputing.SharpBox.Common.Net.oAuth
{
    internal class OAuthService : HttpService
    {
        #region Get Token Method

        private OAuthToken GetToken(String requestTokenUrl)
        {
            // get the token data 
            MemoryStream tokenData = PerformSimpleWebCall(requestTokenUrl, WebRequestMethodsEx.Http.Get, null, null);

            // generate the token as self                       
            return tokenData != null ? OAuthStreamParser.ParseTokenInformation(tokenData) : null;
        }

        public OAuthToken GetRequestToken(OAuthServiceContext svcContext, OAuthConsumerContext conContext)
        {
            // generate the url
            String requestTokenUrl = OAuthUrlGenerator.GenerateRequestTokenUrl(svcContext.RequestTokenUrl, conContext);

            // get the token
            return GetToken(requestTokenUrl);                       
        }

        public OAuthToken GetAccessToken(OAuthServiceContext svcContext, OAuthConsumerContext conContext, OAuthToken requestToken)
        {
            String url = OAuthUrlGenerator.GenerateAccessTokenUrl(svcContext.AccessTokenUrl, conContext, requestToken);

            return GetToken(url);
        }

        #endregion

        #region Special oAuth WebRequest routines

        public virtual WebRequest CreateWebRequest(String url, String method, ICredentials credentials, Object context, OAuthConsumerContext conContext, OAuthToken accessToken, Dictionary<String, String> parameter)
        {
            // generate the signed url
            String signedUrl = GetProtectedResourceUrl(url, conContext, accessToken, parameter, WebRequestMethodsEx.Http.Get);

            // generate the web request as self
            return CreateWebRequest(signedUrl, method, credentials, false, context);
        }

        #endregion
       
        #region Signed URL helpers
        
        public String GetProtectedResourceUrl(String resourceUrl, OAuthConsumerContext conContext, OAuthToken accessToken, Dictionary<String, String> parameter, String webMethod)
        {
            // build url
            return OAuthUrlGenerator.GenerateSignedUrl(resourceUrl, webMethod, conContext, accessToken, parameter);            
        }

        public String GetSignedUrl(String resourceUrl, OAuthConsumerContext conContext, OAuthToken accessToken, Dictionary<String, String> parameter)
        {
            return OAuthUrlGenerator.GenerateSignedUrl(resourceUrl, WebRequestMethodsEx.Http.Post, conContext, accessToken, parameter);
        }

        #endregion
    }
}
