using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Configuration;
using com.bitscopic.hilleman.core.domain;

namespace com.bitscopic.hilleman.core.utils
{
    public static class HttpUtils
    {
        public static Dictionary<String, String> lastRequestHeaders;

        public static String Post(HttpStatusCode expectedResponseCode, Uri baseUri, String resource, String postBody, Dictionary<String, String> headers = null)
        {
            if (null == baseUri)
            {
                baseUri = new Uri(MyConfigurationManager.getValue("CrudSvcBaseUri"));
            }

            WebRequest request = WebRequest.Create(String.Concat(baseUri, resource));
            request.Method = "POST";
            request.ContentType = "application/json";

            if (headers != null && headers.Count > 0)
            {
                foreach (String key in headers.Keys)
                {
                    if (String.Equals(key, "Content-Type", StringComparison.CurrentCultureIgnoreCase))
                    {
                        request.ContentType = headers[key];
                        continue;
                    }
                    request.Headers.Add(key, headers[key]);
                }
            }

            byte[] requestByteAry = System.Text.Encoding.UTF8.GetBytes(postBody);
            request.ContentLength = requestByteAry.Length;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(requestByteAry, 0, requestByteAry.Length);
            requestStream.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            setMyHeadersFromResponse(response);
            Stream stream = response.GetResponseStream();
            StreamReader rdr = new StreamReader(stream);

            if (response.StatusCode == expectedResponseCode)
            {
                String responseBody = rdr.ReadToEnd();
                return responseBody;
            }
            else
            {
                // TODO - handle error
                throw new WebException("Unexpected response status code: " + response.StatusCode);
            }
        }

        public static String Post(Uri baseUri, String resource, String postBody, Dictionary<String, String> headers = null)
        {
            if (null == baseUri)
            {
                baseUri = new Uri(MyConfigurationManager.getValue("CrudSvcBaseUri"));
            }

            WebRequest request = WebRequest.Create(String.Concat(baseUri, resource));
            request.Method = "POST";
            request.ContentType = "application/json";

            if (headers != null && headers.Count > 0)
            {
                foreach (String key in headers.Keys)
                {
                    if (String.Equals(key, "Content-Type", StringComparison.CurrentCultureIgnoreCase))
                    {
                        request.ContentType = headers[key];
                        continue;
                    }
                    request.Headers.Add(key, headers[key]);
                }
            }

            byte[] requestByteAry = System.Text.Encoding.UTF8.GetBytes(postBody);
            request.ContentLength = requestByteAry.Length;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(requestByteAry, 0, requestByteAry.Length);
            requestStream.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            setMyHeadersFromResponse(response);
            Stream stream = response.GetResponseStream();
            StreamReader rdr = new StreamReader(stream);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                String responseBody = rdr.ReadToEnd();
                return responseBody;
            }
            else
            {
                // TODO - handle error
                throw new WebException("Unexpected response status code: " + response.StatusCode);
            }
        }

        public static String Get(Uri baseUri, String resource, Dictionary<String, String> headers)
        {
            if (null == baseUri)
            {
                baseUri = new Uri(MyConfigurationManager.getValue("CrudSvcBaseUri"));
            }

            WebRequest request = WebRequest.Create(String.Concat(baseUri, resource));
            request.Method = "GET";
            if (headers != null && headers.Count > 0)
            {
                foreach (String key in headers.Keys)
                {
                    request.Headers.Add(key, headers[key]);
                }
            }
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            setMyHeadersFromResponse(response);
            Stream stream = response.GetResponseStream();
            StreamReader rdr = new StreamReader(stream);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                String responseBody = rdr.ReadToEnd();
                return responseBody;
            }
            else
            {
                // TODO - handle error
                throw new WebException(System.Enum.GetName(typeof(HttpWebResponse), response.StatusCode));
            }
        }

        public static String Get(Uri baseUri, String resource)
        {
            if (null == baseUri)
            {
                baseUri = new Uri(MyConfigurationManager.getValue("CrudSvcBaseUri"));
            }

            WebRequest request = WebRequest.Create(String.Concat(baseUri, resource));
            request.Method = "GET";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            setMyHeadersFromResponse(response);
            Stream stream = response.GetResponseStream();
            StreamReader rdr = new StreamReader(stream);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                String responseBody = rdr.ReadToEnd();
                return responseBody;
            }
            else
            {
                // TODO - handle error
                throw new WebException(System.Enum.GetName(typeof(HttpWebResponse), response.StatusCode));
            }
        }

        public static String Put(Uri baseUri, String resource, String putBody, Dictionary<String, String> headers = null)
        {
            if (null == baseUri)
            {
                baseUri = new Uri(MyConfigurationManager.getValue("CrudSvcBaseUri"));
            }

            WebRequest request = WebRequest.Create(String.Concat(baseUri, resource));
            request.Method = "PUT";
            request.ContentType = "application/json";

            if (headers != null && headers.Count > 0)
            {
                foreach (String key in headers.Keys)
                {
                    request.Headers.Add(key, headers[key]);
                }
            }

            byte[] requestByteAry = System.Text.Encoding.UTF8.GetBytes(putBody);
            request.ContentLength = requestByteAry.Length;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(requestByteAry, 0, requestByteAry.Length);
            requestStream.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            setMyHeadersFromResponse(response);
            Stream stream = response.GetResponseStream();
            StreamReader rdr = new StreamReader(stream);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                String responseBody = rdr.ReadToEnd();
                return responseBody;
            }
            else
            {
                // TODO - handle error
                throw new WebException(System.Enum.GetName(typeof(HttpWebResponse), response.StatusCode));
            }

        }

        public static String Delete(Uri baseUri, String resource, String deleteBody, Dictionary<String, String> headers = null)
        {
            if (null == baseUri)
            {
                baseUri = new Uri(MyConfigurationManager.getValue("CrudSvcBaseUri"));
            }

            WebRequest request = WebRequest.Create(String.Concat(baseUri, resource));
            request.Method = "DELETE";
            request.ContentType = "application/json";

            if (headers != null && headers.Count > 0)
            {
                foreach (String key in headers.Keys)
                {
                    request.Headers.Add(key, headers[key]);
                }
            }

            if (!String.IsNullOrEmpty(deleteBody))
            {
                byte[] requestByteAry = System.Text.Encoding.UTF8.GetBytes(deleteBody);
                request.ContentLength = requestByteAry.Length;
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(requestByteAry, 0, requestByteAry.Length);
                requestStream.Close();
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            setMyHeadersFromResponse(response);
            Stream stream = response.GetResponseStream();
            StreamReader rdr = new StreamReader(stream);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                String responseBody = rdr.ReadToEnd();
                return responseBody;
            }
            else
            {
                // TODO - handle error
                throw new WebException(System.Enum.GetName(typeof(HttpWebResponse), response.StatusCode));
            }
        }

        static void setMyHeadersFromResponse(HttpWebResponse response)
        {
            if (response.Headers != null && response.Headers.Count > 0)
            {
                HttpUtils.lastRequestHeaders = new Dictionary<string, string>();
                foreach (String headerKey in response.Headers.Keys)
                {
                    HttpUtils.lastRequestHeaders.Add(headerKey, response.Headers[headerKey]);
                }
            }
        }
    }
}
