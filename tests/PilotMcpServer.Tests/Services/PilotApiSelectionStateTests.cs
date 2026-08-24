using PilotMcpServer.Configuration;
using PilotMcpServer.Services;

namespace PilotMcpServer.Tests.Services;

[TestFixture]
public class PilotApiSelectionStateTests
{
    [Test]
    public void PilotApiSelectionState_Current_DefaultsToCatalogDefault_Test()
    {
        var state = new PilotApiSelectionState();

        var current = state.Current;

        Assert.That(current, Is.EqualTo(PilotApiCatalog.Default));
    }

    [Test]
    public void PilotApiSelectionState_SetCurrent_ValidName_UpdatesCurrent_Test()
    {
        var state = new PilotApiSelectionState();

        state.SetCurrent("Python with PostgreSQL");

        Assert.That(state.Current.Name, Is.EqualTo("Python with PostgreSQL"));
        Assert.That(state.Current.Port, Is.EqualTo(55801));
    }

    [Test]
    public void PilotApiSelectionState_SetCurrent_NameIsCaseInsensitive_UpdatesCurrent_Test()
    {
        var state = new PilotApiSelectionState();

        state.SetCurrent("java spring boot with sql server");

        Assert.That(state.Current.Name, Is.EqualTo("Java Spring Boot with SQL Server"));
    }

    [Test]
    public void PilotApiSelectionState_SetCurrent_UnknownName_ThrowsArgumentException_Test()
    {
        var state = new PilotApiSelectionState();

        var ex = Assert.Throws<ArgumentException>(() => state.SetCurrent("Not A Real Api"));

        Assert.That(ex!.Message, Does.Contain("Not A Real Api"));
        Assert.That(ex.Message, Does.Contain(".NET Core with SQL Server"));
    }
}
