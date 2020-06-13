using log4net;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TicketBOT.Middleware
{
    public class ApiLoggingMiddleware : IMiddleware
    {
        private readonly log4net.Core.Level apiLoggingLevel = new log4net.Core.Level(35000, "ReqResp");
        private readonly ILog _logger = LogManager.GetLogger(typeof(ApiLoggingMiddleware));

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                var request = context.Request;
                if (request.Path.StartsWithSegments(new PathString("/api")))
                {
                    var stopWatch = Stopwatch.StartNew();
                    var requestTime = DateTime.UtcNow;
                    var requestBodyContent = await ReadRequestBody(request);
                    var originalBodyStream = context.Response.Body;
                    using (var responseBody = new MemoryStream())
                    {
                        var response = context.Response;
                        response.Body = responseBody;
                        await next(context);
                        stopWatch.Stop();

                        string responseBodyContent = null;
                        responseBodyContent = await ReadResponseBody(response);
                        await responseBody.CopyToAsync(originalBodyStream);

                        SaveReqRespLog(requestTime,
                            stopWatch.ElapsedMilliseconds,
                            response.StatusCode,
                            request.Method,
                            request.Path,
                            request.QueryString.ToString(),
                            requestBodyContent,
                            responseBodyContent);
                    }

                }
                else
                {
                    await next(context);
                }
            }
            catch (Exception)
            {
                await next(context);
            }
        }

        private async Task<string> ReadRequestBody(HttpRequest request)
        {
            request.EnableBuffering();

            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            var bodyAsText = Encoding.UTF8.GetString(buffer);
            request.Body.Seek(0, SeekOrigin.Begin);

            return bodyAsText;
        }

        private async Task<string> ReadResponseBody(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var bodyAsText = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);

            return bodyAsText;
        }

        private void LogRequstResponse(string reqResp)
        {
            _logger.Logger.Log(typeof(ApiLoggingMiddleware), apiLoggingLevel, reqResp, null);
        }

        private void SaveReqRespLog(DateTime requestTime,
                            long responseMillis,
                            int statusCode,
                            string method,
                            string path,
                            string queryString,
                            string requestBody,
                            string responseBody)
        {
            //if (path.ToLower().StartsWith("/api/useraccount/login"))
            //{
            //    requestBody = $"(Request logging disabled for /api/useraccount/login)";
            //    responseBody = $"(Response logging disabled for /api/useraccount/login)";
            //}

            //if (queryString.Length > 500)
            //{
            //    queryString = $"(Truncated to 500 chars) {queryString.Substring(0, 500)}";
            //}

            //if (requestBody.Length > 500)
            //{
            //    requestBody = $"(Truncated to 500 chars) {requestBody.Substring(0, 500)}";
            //}

            if (responseBody.Length > 500)
            {
                responseBody = $"(Truncated to 500 chars) {responseBody.Substring(0, 500)}";
            }

            LogRequstResponse(JsonConvert.SerializeObject(
                new ApiLogItem
                {
                    RequestTime = requestTime,
                    ResponseMillis = responseMillis,
                    StatusCode = statusCode,
                    Method = method,
                    Path = path,
                    QueryString = queryString,
                    RequestBody = requestBody,
                    ResponseBody = responseBody
                }
            ));
        }
    }
}