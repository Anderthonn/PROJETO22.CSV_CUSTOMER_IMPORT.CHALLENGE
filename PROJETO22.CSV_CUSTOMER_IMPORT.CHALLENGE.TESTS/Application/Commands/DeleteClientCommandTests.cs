using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.APPLICATION.Handlers.Commands;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.TESTS
{
    public class DeleteClientCommandTests
    {
        [Fact]
        public void Constructor_Should_Set_Id()
        {
            const int id = 123;

            DeleteClientCommand command = new DeleteClientCommand(id);

            Assert.Equal(id, command.Id);
        }
    }
}