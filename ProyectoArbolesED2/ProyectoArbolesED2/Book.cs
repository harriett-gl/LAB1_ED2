using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class Book
{
    public string Isbn { get; set; }
    public string Name { get; set; }
    public string Author { get; set; }
    public string Category { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public class BTreeNode
{
    public List<Book> Books { get; set; } = new List<Book>();
    public List<BTreeNode> Children { get; set; } = new List<BTreeNode>();
    public bool IsLeaf => Children.Count == 0;
    public int Degree { get; set; }

    public BTreeNode(int degree)
    {
        Degree = degree;
    }
}

public class BTree
{
    private BTreeNode _root;
    private readonly int _t;

    public BTree(int t)
    {
        _t = t;
        _root = new BTreeNode(_t);
    }

    public void Insert(Book book)
    {
        BTreeNode r = _root;
        if (r.Books.Count == 2 * _t - 1)
        {
            BTreeNode s = new BTreeNode(_t);
            _root = s;
            s.Children.Add(r);
            SplitChild(s, 0, r);
            InsertNonFull(s, book);
        }
        else
        {
            InsertNonFull(r, book);
        }
    }

    private void InsertNonFull(BTreeNode x, Book book)
    {
        int i = x.Books.Count - 1;
        if (x.IsLeaf)
        {
            x.Books.Add(null);
            while (i >= 0 && string.Compare(book.Isbn, x.Books[i].Isbn, StringComparison.Ordinal) < 0)
            {
                x.Books[i + 1] = x.Books[i];
                i--;
            }
            x.Books[i + 1] = book;
        }
        else
        {
            while (i >= 0 && string.Compare(book.Isbn, x.Books[i].Isbn, StringComparison.Ordinal) < 0)
            {
                i--;
            }
            i++;
            if (x.Children[i].Books.Count == 2 * _t - 1)
            {
                SplitChild(x, i, x.Children[i]);
                if (string.Compare(book.Isbn, x.Books[i].Isbn, StringComparison.Ordinal) > 0)
                {
                    i++;
                }
            }
            InsertNonFull(x.Children[i], book);
        }
    }

    private void SplitChild(BTreeNode x, int i, BTreeNode y)
    {
        BTreeNode z = new BTreeNode(_t);
        x.Children.Insert(i + 1, z);
        x.Books.Insert(i, y.Books[_t - 1]);
        for (int j = 0; j < _t - 1; j++)
        {
            z.Books.Add(y.Books[j + _t]);
        }
        y.Books.RemoveRange(_t - 1, _t);
        if (!y.IsLeaf)
        {
            for (int j = 0; j < _t; j++)
            {
                z.Children.Add(y.Children[j + _t]);
            }
            y.Children.RemoveRange(_t, _t);
        }
    }

    public void Delete(string isbn)
    {
        Delete(_root, isbn);
        if (_root.Books.Count == 0)
        {
            if (!_root.IsLeaf)
            {
                _root = _root.Children[0];
            }
            else
            {
                _root = new BTreeNode(_t);
            }
        }
    }

    private void Delete(BTreeNode node, string isbn)
    {
        int idx = node.Books.FindIndex(book => book.Isbn == isbn);
        if (idx != -1)
        {
            if (node.IsLeaf)
            {
                node.Books.RemoveAt(idx);
            }
            else
            {
                if (node.Children[idx].Books.Count >= _t)
                {
                    var pred = GetPredecessor(node, idx);
                    node.Books[idx] = pred;
                    Delete(node.Children[idx], pred.Isbn);
                }
                else if (node.Children[idx + 1].Books.Count >= _t)
                {
                    var succ = GetSuccessor(node, idx);
                    node.Books[idx] = succ;
                    Delete(node.Children[idx + 1], succ.Isbn);
                }
                else
                {
                    Merge(node, idx);
                    Delete(node.Children[idx], isbn);
                }
            }
        }
        else
        {
            if (node.IsLeaf)
            {
                return;
            }

            idx = node.Books.FindIndex(book => string.Compare(isbn, book.Isbn, StringComparison.Ordinal) < 0);
            if (idx == -1) idx = node.Books.Count;

            if (idx < node.Children.Count && node.Children[idx].Books.Count < _t)
            {
                Fill(node, idx);
            }

            if (idx > node.Books.Count)
            {
                Delete(node.Children[idx - 1], isbn);
            }
            else
            {
                Delete(node.Children[idx], isbn);
            }
        }
    }

    private Book GetPredecessor(BTreeNode node, int idx)
    {
        BTreeNode current = node.Children[idx];
        while (!current.IsLeaf)
        {
            current = current.Children[current.Books.Count];
        }
        return current.Books[current.Books.Count - 1];
    }

    private Book GetSuccessor(BTreeNode node, int idx)
    {
        BTreeNode current = node.Children[idx + 1];
        while (!current.IsLeaf)
        {
            current = current.Children[0];
        }
        return current.Books[0];
    }

    private void Merge(BTreeNode node, int idx)
    {
        var child = node.Children[idx];
        var sibling = node.Children[idx + 1];

        child.Books.Add(node.Books[idx]);
        child.Books.AddRange(sibling.Books);
        if (!child.IsLeaf)
        {
            child.Children.AddRange(sibling.Children);
        }

        node.Books.RemoveAt(idx);
        node.Children.RemoveAt(idx + 1);
    }

    private void Fill(BTreeNode node, int idx)
    {
        if (idx != 0 && node.Children[idx - 1].Books.Count >= _t)
        {
            BorrowFromPrev(node, idx);
        }
        else if (idx != node.Books.Count && node.Children[idx + 1].Books.Count >= _t)
        {
            BorrowFromNext(node, idx);
        }
        else
        {
            if (idx != node.Books.Count)
            {
                Merge(node, idx);
            }
            else
            {
                Merge(node, idx - 1);
            }
        }
    }

    private void BorrowFromPrev(BTreeNode node, int idx)
    {
        var child = node.Children[idx];
        var sibling = node.Children[idx - 1];

        child.Books.Insert(0, node.Books[idx - 1]);
        if (!child.IsLeaf)
        {
            child.Children.Insert(0, sibling.Children[sibling.Children.Count - 1]);
        }

        node.Books[idx - 1] = sibling.Books[sibling.Books.Count - 1];
        sibling.Books.RemoveAt(sibling.Books.Count - 1);

        if (!sibling.IsLeaf)
        {
            sibling.Children.RemoveAt(sibling.Children.Count - 1);
        }
    }

    private void BorrowFromNext(BTreeNode node, int idx)
    {
        var child = node.Children[idx];
        var sibling = node.Children[idx + 1];

        child.Books.Add(node.Books[idx]);
        if (!child.IsLeaf)
        {
            child.Children.Add(sibling.Children[0]);
        }

        node.Books[idx] = sibling.Books[0];
        sibling.Books.RemoveAt(0);

        if (!sibling.IsLeaf)
        {
            sibling.Children.RemoveAt(0);
        }
    }

    public void Update(string isbn, JObject updates)
    {
        Book book = SearchByIsbn(_root, isbn);
        if (book != null)
        {
            foreach (var update in updates)
            {
                switch (update.Key.ToLower()) // Asegúrate de que la clave esté en minúsculas
                {
                    case "author":
                        book.Author = update.Value.ToString();
                        break;
                    case "category":
                        book.Category = update.Value.ToString();
                        break;
                    case "price":
                        book.Price = decimal.Parse(update.Value.ToString());
                        break;
                    case "quantity":
                        book.Quantity = int.Parse(update.Value.ToString());
                        break;
                }
            }
        }
    }

    public List<Book> SearchByName(string name)
    {
        List<Book> results = new List<Book>();
        SearchByName(_root, name, results);
        return results;
    }

    private void SearchByName(BTreeNode node, string name, List<Book> results)
    {
        foreach (var book in node.Books)
        {
            if (book.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                results.Add(book);
            }
        }

        if (!node.IsLeaf)
        {
            foreach (var child in node.Children)
            {
                SearchByName(child, name, results);
            }
        }
    }

    private Book SearchByIsbn(BTreeNode node, string isbn)
    {
        int i = 0;
        while (i < node.Books.Count && string.Compare(isbn, node.Books[i].Isbn, StringComparison.Ordinal) > 0)
        {
            i++;
        }

        if (i < node.Books.Count && node.Books[i].Isbn == isbn)
        {
            return node.Books[i];
        }

        if (node.IsLeaf)
        {
            return null;
        }

        return SearchByIsbn(node.Children[i], isbn);
    }
}
