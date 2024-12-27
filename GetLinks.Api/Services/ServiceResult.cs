using System.Net;

namespace GetLinks.Api.Services
{
    /// <summary>Representa um retorno do resultado da solicitação de um serviço.</summary>
    public interface IServiceResult
    {
        /// <summary>Obtém se a solicitação foi bem sucedida.</summary>
        bool Succeeded { get; }
        /// <summary>Obtém uma mensagem do resultado da solicitação.</summary>
        string? Message { get; }
        /// <summary>Obtém um objeto relacionado a resposta.</summary>
        object? Data { get; }
        /// <summary>Obtém o código de estado da solicitação.</summary>
        HttpStatusCode StatusCode();
        /// <summary>Obtém o conteúdo da resposta pelo tipo especificado.</summary>
        /// <typeparam name="T">O tipo de dado esperado, onde T é uma classe. Caso o tipo seja inválido retorna null.</typeparam>
        TData? GetData<TData>() where TData : class;
    }

    /// <summary>Representa um retorno do resultado da solicitação de um serviço.</summary>
    public class ServiceResult : IServiceResult
    {
        private readonly HttpStatusCode _statusCode;

        public bool Succeeded { get; protected set; }
        public string? Message { get; protected set; }
        public object? Data { get; protected set; } = null;

        public ServiceResult(bool succeeded, string? message, HttpStatusCode statusCode, object? data = null)
        {
            Succeeded = succeeded;
            Message = message;
            Data = data;
            _statusCode = statusCode;
        }

        public HttpStatusCode StatusCode()
        {
            return _statusCode;
        }

        public virtual T? GetData<T>() where T : class
            => null;
    }

    /// <summary>Representa um retorno do resultado da solicitação de um serviço com um tipo específico.</summary>
    public class ServiceResult<T> : ServiceResult
    {
        public ServiceResult(bool succeeded, string? message, HttpStatusCode statusCode, T? data)
            : base(succeeded, message, statusCode)
        {
            Data = data;
        }

        public override TData? GetData<TData>() where TData : class
        {
            if (Data is TData tdata)
                return tdata;

            return null;
        }
    }
}
