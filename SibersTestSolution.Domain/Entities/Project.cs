namespace SibersTestSolution.Domain.Entities;

/// <summary>
/// Represents a customer project with assigned employees, tasks, and an optional project manager.
/// </summary>
public class Project : Entity
{
    private readonly List<Employee> _employees = [];
    private readonly List<Task> _tasks = [];
    private readonly List<ProjectDocument> _documents = [];

    private Project()
    {
    }

    /// <summary>
    /// Creates a project without assigning a project manager.
    /// </summary>
    public Project(
        string name,
        string customerCompanyName,
        string contractorCompanyName,
        DateTime projectStartDate,
        DateTime projectEndDate,
        int projectPriority)
    {
        SetName(name);
        SetCustomerCompanyName(customerCompanyName);
        SetContractorCompanyName(contractorCompanyName);
        SetProjectStartAndEndDate(projectStartDate, projectEndDate);
        SetProjectPriority(projectPriority);
    }

    /// <summary>
    /// Gets the project name.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets the customer company name.
    /// </summary>
    public string CustomerCompanyName { get; private set; } = null!;

    /// <summary>
    /// Gets the contractor company name.
    /// </summary>
    public string ContractorCompanyName { get; private set; } = null!;

    /// <summary>
    /// Gets the optional identifier of the employee who manages the project.
    /// </summary>
    public int? ProjectManagerId { get; private set; }

    /// <summary>
    /// Gets the optional project manager navigation property.
    /// </summary>
    public Employee? ProjectManager { get; private set; }

    /// <summary>
    /// Gets employees assigned to the project.
    /// </summary>
    public IReadOnlyCollection<Employee> Employees => _employees;

    /// <summary>
    /// Gets tasks that belong to the project.
    /// </summary>
    public IReadOnlyCollection<Task> Tasks => _tasks;

    /// <summary>
    /// Gets documents uploaded for the project.
    /// </summary>
    public IReadOnlyCollection<ProjectDocument> Documents => _documents;

    /// <summary>
    /// Gets the project start date.
    /// </summary>
    public DateTime ProjectStartDate { get; private set; }

    /// <summary>
    /// Gets the project end date.
    /// </summary>
    public DateTime ProjectEndDate { get; private set; }

    /// <summary>
    /// Gets the project priority.
    /// </summary>
    public int ProjectPriority { get; private set; }

    /// <summary>
    /// Adds an employee to the project if it is not already assigned.
    /// </summary>
    public void AddEmployee(Employee employee)
    {
        ArgumentNullException.ThrowIfNull(employee);

        if (_employees.Any(x => x.Id == employee.Id && employee.Id != 0))
        {
            return;
        }

        _employees.Add(employee);
    }

    /// <summary>
    /// Removes an employee from the project by identifier.
    /// </summary>
    public void RemoveEmployee(int employeeId)
    {
        if (employeeId <= 0)
        {
            throw new ArgumentException("Employee id must be greater than 0.", nameof(employeeId));
        }

        var employee = _employees.FirstOrDefault(x => x.Id == employeeId);

        if (employee is not null)
        {
            _employees.Remove(employee);
        }
    }

    /// <summary>
    /// Assigns a project manager by employee identifier.
    /// </summary>
    public void AssignProjectManager(int projectManagerId)
    {
        SetProjectManagerId(projectManagerId);
        ProjectManager = null;
    }

    /// <summary>
    /// Assigns a project manager and keeps the navigation property attached.
    /// </summary>
    public void AssignProjectManager(Employee projectManager)
    {
        ArgumentNullException.ThrowIfNull(projectManager);

        SetProjectManagerId(projectManager.Id);
        ProjectManager = projectManager;
    }

    /// <summary>
    /// Removes the project manager assignment.
    /// </summary>
    public void RemoveProjectManager()
    {
        ProjectManagerId = null;
        ProjectManager = null;
    }

    /// <summary>
    /// Renames the project.
    /// </summary>
    public void Rename(string name)
    {
        SetName(name);
    }

    /// <summary>
    /// Changes the customer and contractor company names.
    /// </summary>
    public void ChangeCompanies(string customerCompanyName, string contractorCompanyName)
    {
        SetCustomerCompanyName(customerCompanyName);
        SetContractorCompanyName(contractorCompanyName);
    }

    /// <summary>
    /// Changes the project date range.
    /// </summary>
    public void ChangePeriod(DateTime projectStartDate, DateTime projectEndDate)
    {
        SetProjectStartAndEndDate(projectStartDate, projectEndDate);
    }

    /// <summary>
    /// Changes the project priority.
    /// </summary>
    public void ChangePriority(int projectPriority)
    {
        SetProjectPriority(projectPriority);
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Project name cannot be empty.", nameof(name));
        }

        Name = name.Trim();
    }

    private void SetCustomerCompanyName(string customerCompanyName)
    {
        if (string.IsNullOrWhiteSpace(customerCompanyName))
        {
            throw new ArgumentException("Customer company name cannot be empty.", nameof(customerCompanyName));
        }

        CustomerCompanyName = customerCompanyName.Trim();
    }

    private void SetContractorCompanyName(string contractorCompanyName)
    {
        if (string.IsNullOrWhiteSpace(contractorCompanyName))
        {
            throw new ArgumentException("Contractor company name cannot be empty.", nameof(contractorCompanyName));
        }

        ContractorCompanyName = contractorCompanyName.Trim();
    }

    private void SetProjectManagerId(int projectManagerId)
    {
        if (projectManagerId <= 0)
        {
            throw new ArgumentException("Project manager id must be greater than 0.", nameof(projectManagerId));
        }

        ProjectManagerId = projectManagerId;
    }

    private void SetProjectStartAndEndDate(DateTime projectStartDate, DateTime projectEndDate)
    {
        if (projectStartDate > projectEndDate)
        {
            throw new ArgumentException("Project start date cannot be later than end date.");
        }

        ProjectStartDate = projectStartDate;
        ProjectEndDate = projectEndDate;
    }

    private void SetProjectPriority(int projectPriority)
    {
        if (projectPriority <= 0)
        {
            throw new ArgumentException("Project priority must be greater than 0.", nameof(projectPriority));
        }

        ProjectPriority = projectPriority;
    }
}
