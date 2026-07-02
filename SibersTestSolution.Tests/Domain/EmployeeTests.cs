using SibersTestSolution.Domain.Entities;

namespace SibersTestSolution.Tests.Domain;

public sealed class EmployeeTests
{
    [Fact]
    public void Constructor_NormalizesFullNameAndEmail()
    {
        var employee = new Employee(" Ivan ", " Ivanov ", " Ivanovich ", " ivan@test.local ");

        Assert.Equal("Ivan", employee.Name);
        Assert.Equal("Ivanov", employee.LastName);
        Assert.Equal("Ivanovich", employee.MiddleName);
        Assert.Equal("ivan@test.local", employee.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-email")]
    [InlineData("ivan@test")]
    public void ChangeEmail_RejectsInvalidEmail(string email)
    {
        var employee = new Employee("Ivan", "Ivanov", "Ivanovich", "ivan@test.local");

        Assert.Throws<ArgumentException>(() => employee.ChangeEmail(email));
    }

    [Theory]
    [InlineData("", "Ivanov", "Ivanovich")]
    [InlineData("Ivan", "", "Ivanovich")]
    [InlineData("Ivan", "Ivanov", "")]
    public void ChangeFullName_RejectsEmptyNamePart(string name, string lastName, string middleName)
    {
        var employee = new Employee("Ivan", "Ivanov", "Ivanovich", "ivan@test.local");

        Assert.Throws<ArgumentException>(() => employee.ChangeFullName(name, lastName, middleName));
    }
}
