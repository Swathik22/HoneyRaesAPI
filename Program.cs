using HoneyRaesAPI.Models;
using HoneyRaesAPI.Models.DTOs;

List<Customer> customers = new List<Customer> 
{
    new Customer { Id = 1, Name = "John Doe", Address = "123 Main Street" },
    new Customer { Id = 2, Name = "Jane Smith", Address = "456 Elm Street" },
    new Customer { Id = 3, Name = "Alice Johnson", Address = "789 Oak Avenue" }

};
List<Employee> employees = new List<Employee> 
{
    new Employee { Id = 1, Name = "Alice Johnson", Specialty = "Project Manager" },
    new Employee { Id = 2, Name = "Bob Williams", Specialty = "Software Developer" }
    

};
List<ServiceTicket> serviceTickets = new List<ServiceTicket> 
{
    new ServiceTicket { Id = 1, EmployeeId = 1, CustomerId = 1, Description = "Network issue", Emergency = false, DateCompleted = DateTime.Parse("2024-04-10") },
    new ServiceTicket { Id = 2, EmployeeId = 2, CustomerId = 2, Description = "Software installation", Emergency = true, DateCompleted = null },
    new ServiceTicket { Id = 3, EmployeeId = 2, CustomerId = 3, Description = "Hardware replacement", Emergency = true, DateCompleted = DateTime.Parse("2024-04-11") },
    new ServiceTicket { Id = 4, EmployeeId = 1, CustomerId = 2, Description = "Database query optimization", Emergency = false, DateCompleted = null },
    new ServiceTicket { Id = 5, EmployeeId = 0, CustomerId = 3, Description = "Email server configuration", Emergency = true, DateCompleted = DateTime.Parse("2024-04-09") }

};



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
//Get all serviceTickets
app.MapGet("/servicetickets", () =>
{
    return serviceTickets.Select(t => new ServiceTicketDTO
    {
        Id = t.Id,
        CustomerId = t.CustomerId,
        EmployeeId = t.EmployeeId,
        Description = t.Description,
        Emergency = t.Emergency,
        DateCompleted = t.DateCompleted
    });
});
//Get serviceTickets by Id
app.MapGet("/servicetickets/{id}",(int id)=>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);

    Employee employee=employees.FirstOrDefault(e=>e.Id==serviceTicket.EmployeeId);
    Customer customer=customers.FirstOrDefault(c=>c.Id==serviceTicket.CustomerId);

    return new ServiceTicketDTO
    {
        Id=serviceTicket.Id,
        CustomerId=serviceTicket.CustomerId,
        CustomerDTO=customer==null?null:new CustomerDTO
        {
            Id=customer.Id,
            Name=customer.Name,
            Address=customer.Address
        },
        EmployeeId=serviceTicket.EmployeeId,
        EmployeeDTO=employee==null?null:new EmployeeDTO
        {
            Id=employee.Id,
            Name=employee.Name,
            Specialty=employee.Specialty
        },
        Description=serviceTicket.Description,
        Emergency=serviceTicket.Emergency,
        DateCompleted=serviceTicket.DateCompleted
    };
});

//Get All employees
app.MapGet("/employees",()=>
{
    return employees.Select(emp=> new EmployeeDTO
    {
        Id=emp.Id,
        Name =emp.Name,
        Specialty=emp.Specialty
    });
});
//Get Employee by Id
app.MapGet("/employees/{id}",(int id)=>{
    Employee emp=employees.FirstOrDefault(e=>e.Id==id);
    if(emp==null)
    {
        return Results.NotFound();
    }
    List<ServiceTicket> tickets=serviceTickets.Where(t=>t.EmployeeId==id).ToList();
    return Results.Ok(new EmployeeDTO
    {
        Id=emp.Id,
        Name=emp.Name,
        Specialty=emp.Specialty,
        serviceTicketDTOs=tickets.Select(t=> new ServiceTicketDTO
        {
            Id=t.Id,
            EmployeeId=t.EmployeeId,
            CustomerId=t.CustomerId,
            Description=t.Description,
            Emergency=t.Emergency,
            DateCompleted=t.DateCompleted
        }).ToList()
    });
});

//Get All Customers
app.MapGet("/customers",()=>{
    return customers.Select(c=>new CustomerDTO
    {
        Id=c.Id,
        Name=c.Name,
        Address=c.Address

    });
});

//Get customer by Id
app.MapGet("/customers/{id}",(int id)=>{
    Customer customer=customers.FirstOrDefault(cust=>cust.Id==id);
    if(customer==null)
    {
        return Results.NotFound();
    }

    return Results.Ok(new CustomerDTO
    {
        Id=customer.Id,
        Name=customer.Name,
        Address=customer.Address
    });
});

//Adding a new Service Ticket
app.MapPost("/servicetickets",(ServiceTicket serviceTicket)=>
{
    // Get the customer data to check that the customerid for the service ticket is valid
    Customer customer=customers.FirstOrDefault(c=>c.Id==serviceTicket.CustomerId);
     // if the client did not provide a valid customer id, this is a bad request
    if(customer==null)
    {
        return Results.BadRequest();
    }
    serviceTicket.Id=serviceTickets.Max(st=>st.Id)+1;
    serviceTickets.Add(serviceTicket);//Adding to the database
    // Created returns a 201 status code with a link in the headers to where the new resource can be accessed
    return Results.Created($"/servicetickets/serviceTicket.Id",new ServiceTicketDTO
    {
        Id=serviceTicket.Id,
        CustomerId=serviceTicket.CustomerId,
        CustomerDTO = new CustomerDTO
        {
            Id = customer.Id,
            Name = customer.Name,
            Address = customer.Address
        },
        Description=serviceTicket.Description,
        Emergency=serviceTicket.Emergency
    });
});

//Delete a service ticket
app.MapDelete("/servicetickets/{id}",(int id)=>
{
    ServiceTicket serviceTicket=serviceTickets.FirstOrDefault(st=>st.Id==id);
    if(serviceTicket!=null)
    {
    serviceTickets.Remove(serviceTicket);
    }
    else
    {
        return Results.NotFound();
    }

    return Results.NoContent();
    
});

//Update a service ticket
app.MapPut("/servicetickets/{id}", (int id, ServiceTicket serviceTicket) =>
{
    ServiceTicket ticketToUpdate = serviceTickets.FirstOrDefault(st => st.Id == id);

    if (ticketToUpdate == null)
    {
        return Results.NotFound();
    }
    if (id != serviceTicket.Id)
    {
        return Results.BadRequest();
    }

    ticketToUpdate.CustomerId = serviceTicket.CustomerId;
    ticketToUpdate.EmployeeId = serviceTicket.EmployeeId;
    ticketToUpdate.Description = serviceTicket.Description;
    ticketToUpdate.Emergency = serviceTicket.Emergency;
    ticketToUpdate.DateCompleted = serviceTicket.DateCompleted;

    return Results.NoContent();
});

app.Run();



