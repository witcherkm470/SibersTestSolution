namespace SibersTestSolution.Domain.Entities;

/// <summary>
/// Represents an employee who can work on projects, manage projects, and participate in tasks.
/// </summary>
public class Employee : Entity
{
    private const string EmailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

    private readonly List<Project> _projects = [];
    private readonly List<Project> _managedProjects = [];
    private readonly List<Task> _createdTasks = [];
    private readonly List<Task> _assignedTasks = [];

    private Employee()
    {
    }

    /// <summary>
    /// Creates an employee with validated name and email data.
    /// </summary>
    public Employee(string name, string lastName, string middleName, string email)
    {
        ChangeFullName(name, lastName, middleName);
        ChangeEmail(email);
    }

    /// <summary>
    /// Gets the employee first name.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets the employee last name.
    /// </summary>
    public string LastName { get; private set; } = null!;

    /// <summary>
    /// Gets the employee middle name.
    /// </summary>
    public string MiddleName { get; private set; } = null!;

    /// <summary>
    /// Gets the employee email address.
    /// </summary>
    public string Email { get; private set; } = null!;

    /// <summary>
    /// Gets a value indicating whether the employee is hidden from active employee lists.
    /// </summary>
    public bool IsDeleted { get; private set; }

    /// <summary>
    /// Gets the projects where the employee is assigned as a worker.
    /// </summary>
    public IReadOnlyCollection<Project> Projects => _projects;

    /// <summary>
    /// Gets the projects managed by the employee.
    /// </summary>
    public IReadOnlyCollection<Project> ManagedProjects => _managedProjects;

    /// <summary>
    /// Gets the tasks created by the employee.
    /// </summary>
    public IReadOnlyCollection<Task> CreatedTasks => _createdTasks;

    /// <summary>
    /// Gets the tasks assigned to the employee.
    /// </summary>
    public IReadOnlyCollection<Task> AssignedTasks => _assignedTasks;

    /// <summary>
    /// Changes the employee full name after validating all required parts.
    /// </summary>
    public void ChangeFullName(string name, string lastName, string middleName)
    {
        SetName(name);
        SetLastName(lastName);
        SetMiddleName(middleName);
    }

    /// <summary>
    /// Changes the employee email address after validating its format.
    /// </summary>
    public void ChangeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Employee email cannot be empty.", nameof(email));
        }

        var normalizedEmail = email.Trim();

        if (!System.Text.RegularExpressions.Regex.IsMatch(normalizedEmail, EmailPattern))
        {
            throw new ArgumentException("Employee email has invalid format.", nameof(email));
        }

        Email = normalizedEmail;
    }

    /// <summary>
    /// Marks the employee as deleted while preserving historical task and project references.
    /// </summary>
    public void MarkDeleted()
    {
        IsDeleted = true;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Employee name cannot be empty.", nameof(name));
        }

        Name = name.Trim();
    }

    private void SetLastName(string lastName)
    {
        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException("Employee last name cannot be empty.", nameof(lastName));
        }

        LastName = lastName.Trim();
    }

    private void SetMiddleName(string middleName)
    {
        if (string.IsNullOrWhiteSpace(middleName))
        {
            throw new ArgumentException("Employee middle name cannot be empty.", nameof(middleName));
        }

        MiddleName = middleName.Trim();
    }
}
