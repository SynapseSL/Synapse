using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Synapse.RCE.Models
{
    public class RceResponse
    {
        public bool IsSuccess { get; set; }
        public RceStatus Status { get; set; }
        public string Content { get; set; }

        internal static RceResponse GetInvalidJsonResponse()
        {
            return new()
            {
                Status = RceStatus.InvalidJson,
                Content = "Invalid Json has been transmitted",
                IsSuccess = false
            };
        }
        internal static RceResponse GetSuccessResponse()
        {
            return new()
            {
                Status = RceStatus.Success,
                Content = "Code compiled and ran sucessfully",
                IsSuccess = true
            };
        }
        internal static RceResponse GetFailedRunResponse(Exception e)
        {
            return new()
            {
                Status = RceStatus.RunFailed,
                Content = $"Exception thrown: {e}",
                IsSuccess = false
            };
        }
        internal static RceResponse GetAssemblyAlreadyLoadedResponse(string name)
        {
            return new()
            {
                Status = RceStatus.AssemblyAlreadyLoaded,
                Content = $"An Assembly with the name \"{name}\" is already loaded",
                IsSuccess = false
            };
        }
        internal static RceResponse GetFailedBuildResponse(Exception e)
        {
            return new()
            {
                Status = RceStatus.CompilationFailed,
                Content = e.ToString(),
                IsSuccess = false
            };
        }
        internal static RceResponse GetFailedBuildResponse(IEnumerable<Diagnostic> failures)
        {
            StringBuilder builder = new();
            foreach (Diagnostic diagnostic in failures)
                builder.AppendLine(string.Format("{0}: {1}", diagnostic?.Id, diagnostic?.GetMessage()));

            return new()
            {
                Status = RceStatus.CompilationFailed,
                Content = builder.ToString(),
                IsSuccess = false
            };
        }
    }
}