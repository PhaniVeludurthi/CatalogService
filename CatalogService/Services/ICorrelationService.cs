namespace CatalogService.Services
{
    public interface ICorrelationService
    {
        public string GetCorrelationId();
        public void SetCorrelationId(string correlationId);
    }
}
