using System.Collections.Generic;

public class BNodo
{
    public List<Article> Articles { get; set; }
    public List<BNodo> Children { get; set; }
    public bool IsLeaf { get; set; }

    public BNodo(bool isLeaf)
    {
        Articles = new List<Article>();
        Children = new List<BNodo>();
        IsLeaf = isLeaf;
    }
}