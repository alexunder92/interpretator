using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

class ParserDemo{
    public static void Main()
    {
        string expr;
        //Parser p=new Parser();
        Console.WriteLine("Для выхода введите \"q\".");

        StreamReader streamReader = new StreamReader("test.txt");
        string str = "";
        Parser p = new Parser();

        double d = 0.0;

        while (!streamReader.EndOfStream)
        {
            str = streamReader.ReadLine();
            d=p.Evaluate(str);
        }

        //double st = p.Evaluate(str);
        Console.WriteLine("Результат:" + d);
        expr = Console.ReadLine();

        while (expr != "q")
        {
            double st = p.Evaluate(expr);
            Console.WriteLine("Результат:" + st);
            expr = Console.ReadLine();
        }
        //Console.ReadLine();



        //expr = "a=2+2";
        //p.Evaluate(expr);
        //expr = "b=5";
        //p.Evaluate(expr);
        //expr = "a+b";
        //double st=p.Evaluate(expr);
        //Console.WriteLine("Результат:" + st);
        //expr = Console.ReadLine();
    }
}


