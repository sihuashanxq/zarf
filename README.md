Zarf是一个基于.Net轻量级的的ORM库,提供了类似于Linq的查询Api.支持SQLite3及MSSQLSERVER数据库

*   [DbContext](#dbcontext)
*   [查询](#query)
    *   [SQL注入](#sqlinject)
    *   [Api](#api)
        *   [IQuey`](#iquey)
        *   [Select](#select)
        *   [Where](#where)
        *   [Join](#join)
        *   [OrderBy](#orderby)
        *   [GroupBy](#groupby)
        *   [Take](#take)
        *   [Skip](#skip)
        *   [All](#all)
        *   [Any](#any)
        *   [Fist/OrDefault](#firstordefault)
        *   [Single/OrDefault](#singleordefault)
        *   [Union](#union)
        *   [Concat](#concat)
        *   [Except](#except)
        *   [Intersect](#intersect)
        *   [Sum](#sum)
        *   [Count](#count)
        *   [Max](#max)
        *   [Min](#min)
        *   [Average](#average)
        *   [ToList](#tolist)
        *   [AsEnumerable](#asenumerable)
    *   [子查询](#sub_query)
        *   [聚合子查询](#aggragesubquery)
        *   [非聚合子查询](#notaggratesubquery)
    *   [函数支持](#function)
        *   [自定义函数](#customfunction)
        *   [内置函数支持](#functionsoupprt)
            *   [简单类型](#simpletype)
            *   [String](#string)
            *   [Math](#math)
    *   [插入](#insert)
    *   [更新](#update)
        *   [属性跟踪](#track)
    *   [删除](#delete)
    *   [SQL合并](#combinesql)
    *   [事务支持](#transaction)
*   [性能测试](#performance)
*   [数据库支持](#database)
*   [协议](#license)

<h2 id="dbcontext">DbContex</h2>

-----
<p>
&emsp;&emsp;DbContext是进行数据查询,插入,删除,修改的上下文对象,一切操作都需要在该上下文对象之下进行操作
</p>

ConoleApp
```c#
using(var db=new DbContext(serviceBuilder=>serviceBuilder.UseSqlServer(ConnectionString)))
{
    foreach(var user in db.Query<User>())
    {

    }
}
```

AspNetCoreApp

```c#
public class MyDbContext : DbContext
{
    public MyDbContext(Func<IDbServiceBuilder, IDbService> serviceBuilder) 
        : base(serviceBuilder)
    {
    }
}

//StartUp.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddTransient(p =>new MyDbContext(b => b.UseSqlServer(ConnectionString)));
}
```

<h2 id="query">查询</h1>

```c#
public class User
{
    public int Id { get; set; }

    public int Age { get; set; }

    public string Name { get; set; }

    public DateTime CreateDay { get; set; }

    public string Tel { get; set; }
}

public class Order
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Goods { get; set; }
}
```

<h4 id="sqlinject">SQL注入</h4>
&emsp;针对SQL注入,对查询中所有常量,进行了参数化,在下面的例子中进行展示

<h3 id="api">Api</h3>

<h4 id="iquey">IQuery`</h4>
&emsp; IQuery接口定义了数据查询的相关Api,使用DbContext中的Query方法进行实例化.

&emsp;IQuery<T>接口没有继承IEnumerable,屏蔽了Linq相关扩展方法的干扰.同时定义了GetEnumerator方法,因此可以使用foreach对IQuery<T>进行迭代访问

```c#
public interface IQuery
{
}

public interface IQuery<TEntity> : IQuery
{
    IQuery<TEntity> Where(Expression<Func<TEntity, bool>> predicate);

    TEntity First();

    TEntity First(Expression<Func<TEntity, bool>> predicate);

    TEntity FirstOrDefault();
    .......
}
```

<h4 id="select">Select<h4>

```c#
db.Query<User>().Select(i => new { i.Id, i.Age, Date = DateTime.Now.Date })
```
```sql
exec sp_executesql N' SELECT  [T0].[Id] AS C5,[T0].[Age] AS C6,@P0 AS C7 FROM [User] AS [T0]',N'@P0 datetime',@P0='2018-02-06 00:00:00'
```

<h4 id="where">Where</h4>

```c#
db.Query<User>().Where(i => i.Id > 10)
```
```sql
exec sp_executesql N' SELECT  [T0].[Id] AS C0,[T0].[Age] AS C1,[T0].[Name] AS C2,[T0].[CreateDay] AS C3,[T0].[Tel] AS C4 FROM [User] AS [T0] WHERE [T0].[Id] > @P0',N'@P0 int',@P0=10
```

<h4 id="join">Join</h4>

```c#
db.Query<User>()
    .Join(db.Query<Order>(), (u, o) => u.Id == o.UserId, JoinType.Inner)
    .Select((u, o) => new { u.Id, o.Goods });
```

```sql
SELECT  [T0].[Id] AS C8,[T1].[Goods] AS C9 FROM [User] AS [T0] Inner JOIN [Order] AS [T1] ON [T0].[Id] = [T1].[UserId]
```

&emsp;Join方法后面只能继续调用Join或者Select方法,如果需要调用Where等方法过滤,需要在Join前调用或者在Select之后调用.

&emsp;提供了InnerJoin,LeftJoin,RightJoin,CorssJoin,FullJoin扩展方法,在子查询中只能使用Join,不能使用InnerJoin等扩展方法

<h4 id="orderby">OrderBy</h4>

```c#
//Age Asc,Id Desc
db.Query<User>().OrderBy(i => i.Age).ThenByDescending(i => i.Id);
```
```sql
 SELECT  [T0].[Id] AS C0,[T0].[Age] AS C1,[T0].[Name] AS C2,[T0].[CreateDay] AS C3,[T0].[Tel] AS C4 FROM [User] AS [T0] ORDER BY [T0].[Age] ASC ,[T0].[Id] DESC 
```

<h4 id="groupby">GroupBy</h4>

```c#
//GroupBy,OrderBy中只能范围一个属性,因此先Select,后续提供i=>i,i=>new{i.Id,i.Age}等支持
db.Query<User>().Select(i => new { i.Id }).GroupBy(i => i.Id);
```
```sql
 SELECT  [T0].[Id] AS C5 FROM [User] AS [T0] GROUP BY [T0].[Id]
```

<h4 id="take">Take</h4>

```c#
db.Query<User>().Take(10);    
```
```sql
 SELECT   TOP  10 [T0].[Id] AS C0,[T0].[Age] AS C1,[T0].[Name] AS C2,[T0].[CreateDay] AS C3,[T0].[Tel] AS C4 FROM [User] AS [T0]
```

<h4 id="skip">Skip</h4>

```c#
//使用ROW_NUMBER()函数,sqlite生成Limit,Offset
db.Query<User>().Skip(2);
```
```sql
exec sp_executesql N' SELECT  [T1].[C0],[T1].[C1],[T1].[C2],[T1].[C3],[T1].[C4] FROM  (  SELECT  [T0].[Id] AS C0,[T0].[Age] AS C1,[T0].[Name] AS C2,[T0].[CreateDay] AS C3,[T0].[Tel] AS C4, ROW_NUMBER() OVER ( ORDER BY GETDATE()) AS __ROWINDEX__ FROM [User] AS [T0] )  AS [T1] WHERE [T1].[__rowIndex__] > @P0',N'@P0 int',@P0=2
```

<h4 id="all">All</h4>
All用于对查询出的所有元素进行判定是否同时满足某些条件,生成Case WHEN NOT EXISTS( NOT )
```c#
db.Query<User>().All(u => u.Id > 0);
```
```sql
exec sp_executesql N' SELECT CASE WHEN  NOT EXISTS(  SELECT  @P0 FROM [User] AS [T0] WHERE  NOT ( [T0].[Id] > @P1 ) ) THEN   CAST(1 AS BIT) ELSE CAST(0 AS BIT) END',N'@P0 bit,@P1 int',@P0=1,@P1=0
```

<h4 id="any">Any</h4>
All用于对查询出的所有元素进行判定是否部分满足某些条件,生成Case WHEN  EXISTS ()
```c#
db.Query<User>().Any(u => u.Id > 10);
```
```sql
exec sp_executesql N' SELECT CASE WHEN  EXISTS(  SELECT  @P0 FROM [User] AS [T0] WHERE [T0].[Id] > @P1 ) THEN   CAST(1 AS BIT) ELSE CAST(0 AS BIT) END',N'@P0 bit,@P1 int',@P0=1,@P1=10
```

<h4 id="first">First/OrDefault</h4>

```c#
db.Query<User>().First(i => i.Id == 3);
db.Query<User>().FirstOrDefault(i => i.Id == 3);
```
```sql
exec sp_executesql N' SELECT   TOP  1 [T0].[Id] AS C0,[T0].[Age] AS C1,[T0].[Name] AS C2,[T0].[CreateDay] AS C3,[T0].[Tel] AS C4 FROM [User] AS [T0] WHERE [T0].[Id] = @P0',N'@P0 int',@P0=3
```

First,FirstOrDefault方法生成的SQL一致,First在没有匹配的数据时抛出异常,FirstOrDefault则返回default(T);

<h4 id="single">Single/OrDefault</h4>

```c#
db.Query<User>().Single(i => i.Id == 3);
db.Query<User>().SingleOrDefault(i => i.Id == 3);
```
```sql
exec sp_executesql N' SELECT   TOP  2 [T0].[Id] AS C0,[T0].[Age] AS C1,[T0].[Name] AS C2,[T0].[CreateDay] AS C3,[T0].[Tel] AS C4 FROM [User] AS [T0] WHERE [T0].[Id] = @P0',N'@P0 int',@P0=3
```
与First,FirstOrDefault类似,不同之处在与返回了2条数据,用于判断是否有重复,如果有重复则抛出异常

<h4 id="union">Union</h4>
并集:不包含重复项,生成Union关键字

```c#
db.Query<User>().Union(db.Query<User>().Where(i=>i.Id<10));
```
```sql
exec sp_executesql N' SELECT  [T0].[Id] AS C0,[T0].[Age] AS C1,[T0].[Name] AS C2,[T0].[CreateDay] AS C3,[T0].[Tel] AS C4 FROM [User] AS [T0] UNION   SELECT  [T1].[Id] AS C5,[T1].[Age] AS C6,[T1].[Name] AS C7,[T1].[CreateDay] AS C8,[T1].[Tel] AS C9 FROM [User] AS [T1] WHERE [T1].[Id] < @P0',N'@P0 int',@P0=10
```

<h4 id="concat">Concat</h4>
并集:与Union效果类似,生成UNION ALL关键字,包含重复项

<h4 id="intersect">Intersect</h4>
交集:与UNION生成SQL类似,生成Intersect关键字

<h4 id="except">Except</h4>
差集:与UNION生成SQL类似,生成Except关键字

<h4 id="sum">Sum</h4>

```c#
db.Query<User>().Sum(i => i.Age);    
```
```sql
 SELECT   TOP  1 Sum([T0].[Age] )  AS C5 FROM [User] AS [T0]
```

<h4 id="count">Count</h4>

```c#
db.Query<User>().Count();    
```
```sql
 SELECT   TOP  1 Count(1 )  AS C5 FROM [User] AS [T0]
```

<h4 id="max">Max</h4>

```c#
db.Query<User>().Max(i=>i.Id);    
```
```sql
 SELECT   TOP  1 Max([T0].[Id] )  AS C5 FROM [User] AS [T0]
```

<h4 id="min">Min</h4>

```c#
db.Query<User>().Min(i=>i.Id);    
```
```sql
 SELECT   TOP  1 Min([T0].[Id] )  AS C5 FROM [User] AS [T0]
```

<h4 id="average">Average</h4>

```c#
db.Query<User>().Average(i=>i.Age);    
```
```sql
 SELECT   TOP  1 CAST( AVG([T0].[Age] )  As Float )  AS C5 FROM [User] AS [T0]
```

<h4 id="tolist">ToList</h4>
&emsp;ToList调用会导致一次正式的查询,同时将数据拷贝到内存中,调用该方法不会生成额外的SQL语句

<h4 id="asenumerable">AsEnumerable</h4>
&emsp;AsEnumerable方法调用不会导致一次查询,正式查询将在对其调用GetEnumerator方法,ToList,foreach时触发.

<h3 id="sub_query">子查询</h3>
&emsp;子查询只能出现在Select方法中,用于复杂查询.同时子查询如果时非聚合类的查询,无法缓存创建实体的委托,性能较低.

&emsp;子查询必须以Sum,Max,Min,Average,Count,First/OrDefault,Single/OrDefault,ToList,AsEnumerable结尾,不能返回IQuery接口

<h4 id="aggragesubquery">聚合子查询</h4>
聚合子查询不会生成两条查询语句,会合并到外层查询中进行查询,可以重用实体创建委托
```c#
 //在子查询中如果是聚合类的查询,则合并到外层查询中
 db.Query<User>()
    .Where(i => i.Id < 10)
    .Select(i => new
    {
        UserId = i.Id,
        MaxUserId = db.Query<User>().Where(m => m.Id == i.Id).Max(m => m.Id)
    });
```
```sql
exec sp_executesql N' SELECT  [T0].[Id],[T0].[Id] AS C5,(  SELECT   TOP  1 Max([T1].[Id] )  AS C12 FROM [User] AS [T1] WHERE [T1].[Id] = [T0].[Id] )  AS C13 FROM [User] AS [T0] WHERE [T0].[Id] < @P0',N'@P0 int',@P0=10
```

<h4 id="notaggratesubquery">非聚合子查询</h4>

```c#
db.Query<User>()
    .Where(i => i.Id < 10)
    .Select(i => new
    {
        UserId = i.Id,
        Orders = db.Query<Order>().Where(o => o.UserId == i.Id).ToList(),
        Orders2=db.Query<Order>().Where(o => o.UserId == i.Id).AsEnumerable()
    });   
```

```sql
--外层查询
exec sp_executesql N' SELECT  [T0].[Id] AS [C9],[T0].[Id] AS [C13],[T0].[Id] AS C5 FROM [User] AS [T0] WHERE [T0].[Id] < @P0',N'@P0 int',@P0=10

--内层Orders查询
exec sp_executesql N' SELECT  [T1].[Id] AS C6,[T1].[UserId] AS C7,[T1].[Goods] AS C8,[T0].[C9] FROM [Order] AS [T1] Cross JOIN  (  SELECT  [T0].[Id] AS [C9] FROM [User] AS [T0] WHERE [T0].[Id] < @P0 )   AS [T0] WHERE [T1].[UserId] = [T0].[C9] GROUP BY [T1].[Id],[T1].[UserId],[T1].[Goods],[T0].[C9]',N'@P0 int',@P0=10

--内层Orders2将延迟到第一次调用Orders2的GetEnumerator时触发,只查询一次
```
&emsp;针对非聚合类的子查询则采用分多次查询的方式,在内存中过滤,ToList相对与AsEnumerable方法,在查询时的开销较多.

&emsp;如果引用了外层查询的字段,在子查询中会自动关联该条件进行Corss Join过滤

&emsp;在进行子查询前,尽量对外层查询进行过滤,因为在子查询前的条件过滤,子查询中会包含该条件.在子查询之进行的过滤,子查询中不会包含

&emsp;这种类型的查询无法返回委托,需要多次创建,性能较低

&emsp;在子查询中进行Join调用时,不能引用外部查询字段,需要提交关联或者延迟后Select调用之后

&emsp;由于在内存中进行过滤子查询,会生成一个子查询返回类型的子类,因此在子查询中不能出现匿名类型及密封类型,如果没有引用外部字段,则无所谓

<h3 id="function">函数支持</h3>
提供了实现自定义函数的能力(只支持参数,返回值都时简单类型的函数)

<h4 id="customfunction">自定义函数</h4>
```c#
/// <summary>
/// int 扩展
/// </summary>
public static class IntExtension
{
    [SQLFunctionHandler(typeof(IntSQLFunctionHandler))]
    public static int Add(this int i, int n)
    {
        //如果没有常数调用(Add(1,2),1.Add(2))的情况,可以抛出异常
        return i + n;
    }
}

/// <summary>
/// 处理Function的SQL生成
/// </summary>
public class IntSQLFunctionHandler : ISQLFunctionHandler
{
    public Type SoupportedType => typeof(int);

    public bool HandleFunction(ISQLGenerator generator, MethodCallExpression methodCall)
    {
        //SQL 生成,调用generator可以对常量进行参数化处理
        if (methodCall.Method.Name == "Add")
        {
            generator.Attach(methodCall.Arguments[0]);
            generator.Attach(" + ");
            generator.Attach(methodCall.Arguments[1]);
            return true;
        }

        return false;
    }
}

//使用
db.Query<User>().Where(i => i.Id.Add(2) < 10).Select(i => new { Id = i.Id.Add(3), Name = i.Name});

```

```sql
exec sp_executesql N' SELECT  [T0].[Id] + @P0 AS C5,[T0].[Name] AS C6 FROM [User] AS [T0] WHERE [T0].[Id] + @P1 < @P2',N'@P0 int,@P1 int,@P2 int',@P0=3,@P1=2,@P2=10
```

<h4 id="functionsoupprt">内置函数支持</h4>

<h5 id="simpletype">简单类型</h5>
支持全部常见简单类型的Equals,Parse,ToString方法

```c#
int,int?char,char?,double,double?,short,short?,byte,byte?,bool,bool?
decimal,decimal?,DateTime,DateTime?uint,uint?ulong,ulong?,float,float?
ushort,ushort?,string
```

如:
```c#
db.Query<User>().Select(i => i.Id.ToString());
```
```sql
 SELECT   CAST ([T0].[Id] AS NVARCHAR ) FROM [User] AS [T0]
```

<h5 id="string">String</h5>

```c#
    IsNullOrEmpty
    IsNullOrWhiteSpace
    StartsWith
    EndsWith
    Contains
    Trim
    TrimStart
    TrimEnd
    IndexOf
    Substring
    ToLower
    ToUpper
    Replace
    Concat
```

```c#
//Id包含1
db.Query<User>().Select(i => i.Id.ToString().Contains("1"));
```

```sql
exec sp_executesql N' SELECT   ( CASE WHEN   CAST ([T0].[Id] AS NVARCHAR ) LIKE ''%''+ @P0 +''%'' THEN @P1 ELSE @P2 END )  FROM [User] AS [T0]',N'@P0 nvarchar(1),@P1 bit,@P2 bit',@P0=N'1',@P1=1,@P2=0
```
<h5 id="math">Math</h5>
支持所有System.Math类中定义的静态方法,SQLite采用了一些垫片

```c#
    Abs,Acos,Asin
    Atan,Cos,Ceiling
    Floor,Exp,Log10
    Sign,Sin,Sqrt
    Tan,Atan2,Log
    Max,Min,PowRound,
    SinhCosh,TanhTruncate
```

```c#
db.Query<User>().Select(i => Math.Max(i.Id, i.Age));
```

```sql
 SELECT  CASE WHEN [T0].[Id] > [T0].[Age] THEN [T0].[Id] ELSE  [T0].[Age] END  FROM [User] AS [T0]
```

<h3 id="#insert">插入</h3>

<!-- 
  *   [插入](#insert)
    *   [更新](#update)
        *   [属性跟踪](#track)
    *   [删除](#delete)
    *   [SQL合并](#combinesql)
    *   [事务支持](#transaction)
*   [性能测试](#performance)
*   [数据库支持](#database) -->

<h2 id="license">协议</h1>
MIT
