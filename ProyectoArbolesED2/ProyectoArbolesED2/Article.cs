using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Article
{
    public string ISBN { get; set; }
    public string Name { get; set; }
    public string Author { get; set; }
    public string Category { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }

    public override string ToString()
    {
        return $"{{\"isbn\":\"{ISBN}\",\"name\":\"{Name}\",\"author\":\"{Author}\",\"category\":\"{Category}\",\"price\":\"{Price:0.00}\",\"quantity\":\"{Quantity}\"}}";
    }
}