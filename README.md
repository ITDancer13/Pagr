# Pagr based on Sieve

Pagr is a fork from [Sieve](https://github.com/Biarity/Sieve) created by [Biarity](https://github.com/Biarity). As Pagr is a very young fork, it's  compatible with the original project, but all occurences of *Sieve* in classes, interfaces or methods are replaced by *Pagr*.

## What is Pagr

Pagr is a simple, clean, and extensible framework for .NET Core that **adds sorting, filtering, and pagination functionality out of the box**.  Most common use case would be for serving ASP.NET Core GET queries.

## Usage for ASP.NET Core

In this example, consider an app with a `Post` entity. 
We'll use Pagr to add sorting, filtering, and pagination capabilities when GET-ing all available posts.

### 1. Add required services

Inject the `PagrProcessor` service. So in `Startup.cs` add:
```C#
services.AddScoped<PagrProcessor>();
```

### 2. Tell Sieve which properties you'd like to sort/filter in your models

Pagr will only sort/filter properties that have the attribute `[Pagr(CanSort = true, CanFilter = true)]` on them (they don't have to be both true).
So for our `Post` entity model example:
```C#
public int Id { get; set; }

[Pagr(CanFilter = true, CanSort = true)]
public string Title { get; set; }

[Pagr(CanFilter = true, CanSort = true)]
public int LikeCount { get; set; }

[Pagr(CanFilter = true, CanSort = true)]
public int CommentCount { get; set; }

[Pagr(CanFilter = true, CanSort = true, Name = "created")]
public DateTimeOffset DateCreated { get; set; } = DateTimeOffset.UtcNow;

```
There is also the `Name` parameter that you can use to have a different name for use by clients.

Alternatively, you can use [Fluent API](#fluent-api) to do the same. This is especially useful if you don't want to use attributes or have multiple APIs. 

### 3. Get sort/filter/page queries by using the Pagr model in your controllers

In the action that handles returning Posts, use `PagrModel` to get the sort/filter/page query. 
Apply it to your data by injecting `PagrProcessor` into the controller and using its `Apply<TEntity>` method. So for instance:
```C#
[HttpGet]
public JsonResult GetPosts(PagrModel pagrModel) 
{
    var result = _dbContext.Posts.AsNoTracking(); // Makes read-only queries faster
    result = _pagrProcessor.Apply(pagrModel, result); // Returns `result` after applying the sort/filter/page query in `PagrModel` to it
    return Json(result.ToList());
}
```
You can also explicitly specify if only filtering, sorting, and/or pagination should be applied via optional arguments.

### 4. Send a request

[Send a request](#send-a-request)

### Add custom sort/filter methods

If you want to add custom sort/filter methods, inject `IPagrCustomSortMethods` or `IPagrCustomFilterMethods` with the implementation being a class that has custom sort/filter methods that Pagr will search through.

For instance:
```C#
services.AddScoped<IPagrCustomSortMethods, PagrCustomSortMethods>();
services.AddScoped<IPagrCustomFilterMethods, PagrCustomFilterMethods>();
```
Where `PagrCustomSortMethodsOfPosts` for example is:
```C#
public class PagrCustomSortMethods : IPagrCustomSortMethods
{
    public IQueryable<Post> Popularity(IQueryable<Post> source, bool useThenBy, bool desc) // The method is given an indicator of weather to use ThenBy(), and if the query is descending 
    {
        var result = useThenBy ?
            ((IOrderedQueryable<Post>)source).ThenBy(p => p.LikeCount) : // ThenBy only works on IOrderedQueryable<TEntity>
            source.OrderBy(p => p.LikeCount)
            .ThenBy(p => p.CommentCount)
            .ThenBy(p => p.DateCreated);

        return result; // Must return modified IQueryable<TEntity>
    }

    public IQueryable<T> Oldest<T>(IQueryable<T> source, bool useThenBy, bool desc) where T : BaseEntity // Generic functions are allowed too
    {
        var result = useThenBy ?
            ((IOrderedQueryable<T>)source).ThenByDescending(p => p.DateCreated) :
            source.OrderByDescending(p => p.DateCreated);

        return result;
    }
}
```
And `PagrCustomFilterMethods`:
```C#
public class PagrCustomFilterMethods : IPagrCustomFilterMethods
{
    public IQueryable<Post> IsNew(IQueryable<Post> source, string op, string[] values) // The method is given the {Operator} & {Value}
    {
        var result = source.Where(p => p.LikeCount < 100 &&
                                        p.CommentCount < 5);

        return result; // Must return modified IQueryable<TEntity>
    }

    public IQueryable<T> Latest<T>(IQueryable<T> source, string op, string[] values) where T : BaseEntity // Generic functions are allowed too
    {
        var result = source.Where(c => c.DateCreated > DateTimeOffset.UtcNow.AddDays(-14));
        return result;
    }
}
```

## Configure Pagr
Use the [ASP.NET Core options pattern](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options) with `PagrOptions` to tell Pagr where to look for configuration. For example:
```C#
services.Configure<PagrOptions>(Configuration.GetSection("Pagr"));
```
Then you can add the configuration:
```json
{
    "Pagr": {
        "CaseSensitive": "boolean: should property names be case-sensitive? Defaults to false",
        "DefaultPageSize": "int number: optional number to fallback to when no page argument is given. Set <=0 to disable paging if no pageSize is specified (default).",
        "MaxPageSize": "int number: maximum allowed page size. Set <=0 to make infinite (default)",
        "ThrowExceptions": "boolean: should Pagr throw exceptions instead of silently failing? Defaults to false"
    }
}
```

## Send a request

With all the above in place, you can now send a GET request that includes a sort/filter/page query.
An example:
```curl
GET /GetPosts

?sorts=     LikeCount,CommentCount,-created         // sort by likes, then comments, then descendingly by date created 
&filters=   LikeCount>10, Title@=awesome title,     // filter to posts with more than 10 likes, and a title that contains the phrase "awesome title"
&page=      1                                       // get the first page...
&pageSize=  10                                      // ...which contains 10 posts

```
More formally:
* `sorts` is a comma-delimited ordered list of property names to sort by. Adding a `-` before the name switches to sorting descendingly.
* `filters` is a comma-delimited list of `{Name}{Operator}{Value}` where
    * `{Name}` is the name of a property with the Pagr attribute or the name of a custom filter method for TEntity
        * You can also have multiple names (for OR logic) by enclosing them in brackets and using a pipe delimiter, eg. `(LikeCount|CommentCount)>10` asks if `LikeCount` or `CommentCount` is `>10`
    * `{Operator}` is one of the [Operators](#operators)
    * `{Value}` is the value to use for filtering. If the filter contains a comma it must be, escaped **\\,**
        * You can also have multiple values (for OR logic) by using a pipe delimiter, eg. `Title@=new|hot` will return posts with titles that contain the text "`new`" or "`hot`"
* `page` is the number of page to return
* `pageSize` is the number of items returned per page 

Notes:
* You can use backslashes to escape commas and pipes within value fields
* You can have spaces anywhere except *within* `{Name}` or `{Operator}` fields
* If you need to look at the data before applying pagination (eg. get total count), use the optional paramters on `Apply` to defer pagination (an [example](https://github.com/Biarity/Sieve/issues/34))
* Here's a [good example on how to work with enumerables](https://github.com/Biarity/Sieve/issues/2)
* Another example on [how to do OR logic](https://github.com/Biarity/Sieve/issues/8)

### Nested objects
You can filter/sort on a nested object's property by marking the property using the Fluent API. 
Marking via attributes not currently supported.

For example, using this object model:

```C#
public class Post {
    public User Creator { get; set; }
}

public class User {
    public string Name { get; set; }
}
```

Mark `Post.User` to be filterable:
```C#
// in MapProperties
mapper.Property<Post>(p => p.Creator.Name)
    .CanFilter();
```

Now you can make requests such as: `filters=User.Name==specific_name`.

### Creating your own DSL
You can replace this DSL with your own (eg. use JSON instead) by implementing an IPagrModel. You can use the default PagrModel for reference.

### Operators
| Operator   | Meaning                  |
|------------|--------------------------|
| `==`       | Equals                   |
| `!=`       | Not equals               |
| `>`        | Greater than             |
| `<`        | Less than                |
| `>=`       | Greater than or equal to |
| `<=`       | Less than or equal to    |
| `@=`       | Contains                 |
| `_=`       | Starts with              |
| `!@=`      | Does not Contains        |
| `!_=`      | Does not Starts with     |
| `@=*`      | Case-insensitive string Contains |
| `_=*`      | Case-insensitive string Starts with |
| `==*`      | Case-insensitive string Equals |
| `!=*`      | Case-insensitive string Not equals |
| `!@=*`     | Case-insensitive string does not Contains |
| `!_=*`     | Case-insensitive string does not Starts with |

### Handle Pagr's exceptions

Pagr will silently fail unless `ThrowExceptions` in the configuration is set to true. 3 kinds of custom exceptions can be thrown:

* `PagrMethodNotFoundException` with a `MethodName`
* `PagrIncompatibleMethodException` with a `MethodName`, an `ExpectedType` and an `ActualType`
* `PagrException` which encapsulates any other exception types in its `InnerException`

It is recommended that you write exception-handling middleware to globally handle Pagr's exceptions when using it with ASP.NET Core.


### Example project
You can find an example project incorporating most Pagr concepts in [Pagr.Sample](https://github.com/uhlpac/Pagr/tree/master/Pagr.Sample).

## Fluent API
To use the Fluent API instead of attributes in marking properties, setup an alternative `PagrProcessor` that overrides `MapProperties`. For example:

```C#
public class ApplicationPagrProcessor : PagrProcessor
{
    public ApplicationPagrProcessor(
        IOptions<PagrOptions> options, 
        IPagrCustomSortMethods customSortMethods, 
        IPagrCustomFilterMethods customFilterMethods) 
        : base(options, customSortMethods, customFilterMethods)
    {
    }

    protected override PagrPropertyMapper MapProperties(PagrPropertyMapper mapper)
    {
        mapper.Property<Post>(p => p.Title)
            .CanFilter()
            .HasName("a_different_query_name_here");

        mapper.Property<Post>(p => p.CommentCount)
            .CanSort();

        mapper.Property<Post>(p => p.DateCreated)
            .CanSort()
            .CanFilter()
            .HasName("created_on");

        return mapper;
    }
}
```

Now you should inject the new class instead:
```C#
services.AddScoped<IPagrProcessor, ApplicationPagrProcessor>();
```

## License & Contributing
Pagr is licensed under Apache 2.0. Any contributions highly appreciated!
Please respect our [Code of Conduct](https://github.com/uhlpac/Pagr/blob/build/CODE_OF_CONDUCT.md)