using System;

//Класс исключений для обнаружения ошибок анализатора
class ParserException:ApplicationException{
    public ParserException(string str):base(str){ }
    public override string ToString(){
        return Message;
    }
}

class Parser{
    enum Types { NONE, DELIMITER, VARIABLE, NUMBER, ARRAY, SORT, WRITE };//Перечисляем типы лексем.
    enum Errors { SYNTAX, UNBALPARENS, NOEXP, DIVBYZERO, SYNTAXINARRAY }; //Перечисляем типы ошибок.
    //enum KeyWords { IF };

    string exp;//Ссылка на строку выражения,
    int expIdx;//Текущий индекс в выражении,
    string token;//Текущая лексема.
    Types tokType;//Тип лексемы.

    //Массив для переменных
    static int vararr = 100;
    double[] vars=new double[26];
    double[,] arr = new double[26, vararr];
    public Parser(){
        //Инициализируем переменные нулевыми значениями.
        for(int i=0;i<vars.Length;i++)
        vars[i]=0.0;

        for (int i = 0; i < vars.Length; i++)
            for (int j = 0; j < vararr; j++) arr[i, j] = 0.0;

    }
   
    //Входная точка анализатора.
    public double Evaluate(string expstr)
    {
        double result;
        exp=expstr;
        expIdx=0;
            
        try
        {
            GetToken();
            if(token==""){
                SyntaxErr(Errors.NOEXP);//Выражение отсутствует
                return 0.0;
            }

            EvalExp1(out result);
            if(token!="")//Последняя лексема должна быть null-значением.
                SyntaxErr(Errors.SYNTAX);
                return result;
            }
            catch(ParserException exc)
            {
                //Надо добавить сюда обработку других ошибок.
                Console.WriteLine(exc);
                return 0.0;
            }
        }

    //Обрабатываем присвоение
    void EvalExp1(out double result)
    {
        int varIdx,arrIdx;
        Types ttokType;
        string temptoken;
        if (tokType == Types.VARIABLE)
        {
            //Сохраняем старую лексему
            temptoken = string.Copy(token);
            ttokType = tokType;
            //Вычисляем индекс переменной
            varIdx = Char.ToUpper(token[0]) - 'A';
            GetToken();
            if (token != "=")
            {
                PutBack();//Возвращаем текущую лексему в поток и восстанавливаем старую
                token = String.Copy(temptoken);
                tokType = ttokType;
            }
            else
            {
                GetToken();//Получаем следующую часть выражения ехр.
                EvalExp2(out result);
                vars[varIdx] = result;
                return;
            }
        }

        if (tokType == Types.ARRAY)
        {
            //Сохраняем старую лексему
            temptoken = string.Copy(token);
            ttokType = tokType;
            //Проверяем правильность ввода массива.
            GetToken();
            if (token == "")// arr
            {
                SyntaxErr(Errors.SYNTAXINARRAY);
            }
            else
            if (!Char.IsLetter(token[0])) SyntaxErr(Errors.SYNTAXINARRAY);
            else
            {
                arrIdx = Char.ToUpper(token[0]) - 'A';
                GetToken();
                if (token != "=")
                {
                    PutBack();//Возвращаем текущую лексему в поток и восстанавливаем старую
                    token = String.Copy(temptoken);
                    tokType = ttokType;
                }
                else
                {
                    GetToken();//Получаем следующую часть массива.
                    if (token == "[")
                    {
                        //Заполняем массив элементами и считаем количество элементов в массиве.
                        GetToken();
                        while (tokType == Types.NUMBER&&token!="]")
                        {
                            arr[arrIdx, 0]++;// количество элементов в массиве будем хранить в нулевом элементе массива
                            EvalExp2(out result);
                            arr[arrIdx, (int)arr[arrIdx, 0]] = result;
                            GetToken();
                            if (token == ";") GetToken(); //получаем следующее число
                      
                        }
                        if (token != "") SyntaxErr(Errors.SYNTAXINARRAY);// после элемента массива непонятный символ

                    }
                    else SyntaxErr(Errors.SYNTAXINARRAY);
                }
                


            }
        }

        EvalExp2(out result);
    }

    //Сложение или вычитание двух членов выражения.
    void EvalExp2(out double result)
    {
        string op;
        double partialResult;

        EvalExp3(out result);
        while((op=token)=="+"||op=="-")
        {
            GetToken();
            EvalExp3(out partialResult);
            switch(op){
                case"-":
                    result=result-partialResult;
                    break;
                case"+":
                    result=result+partialResult;
                    break;
            }
        }
    }

//Умножение или деление двух множителей.
void EvalExp3(out double result )
{
    string op;
    double partialResult=0.0;
                    
    EvalExp4(out result);
    while((op=token)=="*"||op=="/"||op=="%"){
        GetToken();
        EvalExp4(out partialResult);
        switch(op){
            case"*":
                result=result*partialResult;
                break;
            case"/":
                if(partialResult==0.0) SyntaxErr(Errors.DIVBYZERO);
                result=result/partialResult;
                break;
            case"%":
                if(partialResult==0.0) SyntaxErr(Errors.DIVBYZERO);
                result=(int)result%(int)partialResult;
                break;
        }
    }
}

//Возведение в степень.
void EvalExp4(out double result)
{
    double partialResult,ex;
    int t;
    EvalExp5(out result);
    if(token=="^"){
        GetToken();
        EvalExp4(out partialResult);
        ex=result;
        if(partialResult==0.0 ){
            result=1.0;
            return;
        }
        for(t=(int)partialResult-1;t>0;t--)
            result=result*(double)ex;
    }
}

void EvalExp5(out double result)
{
    double partialResult, ex;
    int t;
    EvalExp6(out result);
    if (token == "&")
    {
        GetToken();
        EvalExp5(out partialResult);
        ex = result;
        if (partialResult == 0.0)
        {
            result = 0.0;
            return;
        }
        //t = (int)partialResult;
        for (t = (int)partialResult; t > 0; t--)
        {
            Parser oo = new Parser();
            oo.vars = this.vars;
            string[] str = exp.Split('&');

            result=oo.Evaluate(str[0]);
            this.vars = oo.vars;
        }
    }
}
//Выполнение операции унарного + или -.
void EvalExp6(out double result){
    string op;

    op="";
    if((tokType==Types.DELIMITER)&&token=="+"||token=="-"){
        op=token;
        GetToken();
    }
    EvalExp7(out result);
    if(op=="-") result=-result;
}

//Обработка выражения в круглых скобках.
void EvalExp7(out double result)
{
    if((token=="(")){
        GetToken();
        EvalExp2(out result);
        if(token!=")") SyntaxErr(Errors.UNBALPARENS);
        GetToken();
    }
else Atom(out result);
}

//Получаем значение числа или переменной
void Atom(out double result)
{
    switch(tokType){
        case Types.NUMBER:
            try{
                result=Double.Parse(token);
            }catch(FormatException){
                result=0.0;
                SyntaxErr(Errors.SYNTAX);
            }
            GetToken();
            return;
        case Types.VARIABLE:
            result=FindVar(token);
            GetToken();
            return;


        default:
        result=0.0;
        SyntaxErr(Errors.SYNTAX);
        break;
    }
}

//Возвращаем значение переменной
double FindVar(string vname)
{
    if(!Char.IsLetter(vname[0])){
        SyntaxErr(Errors.SYNTAX);
        return 0.0;
    }
return vars[Char.ToUpper(vname[0])-'A'];
}

//Возвращаем лексему во входной поток.
void PutBack()
{
    for(int i=0;i<token.Length;i++)expIdx--;
}

//Обрабатываем синтаксическую ошибку.
void SyntaxErr(Errors error)
{
    string[] err={
    "Синтаксическая ошибка",
    "Дисбаланс скобок",
    "Выражение отсутствует",
    "Деление на нуль",
    "Синтаксическая ошибка в задании массива"
    };

    throw new ParserException(err[(int) error]);
}

//Получаем следующую лексему.
void GetToken()
{
    tokType=Types.NONE;
    token="";
    if(expIdx==exp.Length) return;//конец выражения
    
    //Пропускаем пробелы,
    while(expIdx<exp.Length&&Char.IsWhiteSpace(exp[expIdx]))++expIdx;
    
    //Хвостовой пробел завершает выражение
    if(expIdx==exp.Length) return;

    if(IsDelim(exp[expIdx])){//Это оператор?
        token+=exp[expIdx];
        expIdx++;
        tokType=Types.DELIMITER;}
        else if(Char.IsLetter(exp[expIdx])){//Это переменная?
            while(!IsDelim(exp[expIdx])){
                token+=exp[expIdx];
                expIdx++;
                if (token.IndexOf("arr") != -1)
                { tokType = Types.ARRAY; break; }
                else if (token.IndexOf("sort") != -1)
                { tokType = Types.SORT; break; }
                else if (token.IndexOf("write") != -1)
                { tokType = Types.WRITE; break; }
                else tokType = Types.VARIABLE;
                if(expIdx>=exp.Length) break;
            }
           
        }
    else if(Char.IsDigit(exp[expIdx])){//Это число?
        while(!IsDelim(exp[expIdx])){
            token+=exp[expIdx];
            expIdx++;
            if(expIdx>=exp.Length) break;
        }
        tokType=Types.NUMBER;
    }
}

//Метод возвращает значение true,если символ с является разделителем
    bool IsDelim(char с)
    {
        if(("+-/*%^=()&[];".IndexOf(с)!=-1))
        return true;
    return false;
    }
}














