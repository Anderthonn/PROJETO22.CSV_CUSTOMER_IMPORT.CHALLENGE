using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Handlers.Queries;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.TESTS.Application.Queries
{
    public class GetByIdClientQueryTests
    {
        [Fact]
        public void Constructor_Stores_Id()
        {
            int id = 42;

            GetByIdClientQuery query = new GetByIdClientQuery(id);

            Assert.Equal(id, query.Id);
        }
    }
}