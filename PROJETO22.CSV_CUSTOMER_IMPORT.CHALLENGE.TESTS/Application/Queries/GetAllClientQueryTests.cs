using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Handlers.Queries;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.TESTS.Application.Queries
{
    public class GetAllClientQueryTests
    {
        [Fact]
        public void Constructor_ShouldCreateInstance()
        {
            GetAllClientQuery query = new GetAllClientQuery();

            Assert.NotNull(query);
        }
    }
}