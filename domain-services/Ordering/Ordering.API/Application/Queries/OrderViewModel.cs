namespace Ordering.API.Application.Queries;

public record Orderitem
{
    public string Productname { get; init; }
    public int Units { get; init; }
    public double Unitprice { get; init; }
    public string Pictureurl { get; init; }
}

public record Order
{
    public int Ordernumber { get; init; }
    public DateTime Date { get; init; }
    public string Status { get; init; }
    public string Description { get; init; }
    public string Street { get; init; }
    public string City { get; init; }
    public string Zipcode { get; init; }
    public string Country { get; init; }
    public List<Orderitem> Orderitems { get; set; }
    public decimal Total { get; set; }
}

public record OrderSummary
{
    public int Ordernumber { get; init; }
    public DateTime Date { get; init; }
    public string Status { get; init; }
    public double Total { get; init; }
}

public record CardType
{
    public int Id { get; init; }
    public string Name { get; init; }
}