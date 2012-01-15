Download from NuGet - **PM> Install-Package NetBash.Database**

Simple commands to query your database with NetBash. ConnectionString name can be set with --conn=[yourdb] if it isnt set it will default to the first one found.
    
    Usage: sql [OPTIONS] [QUERY]
    Execute sql on your database using NetBash
    
    Options:
      -e, --execute              Executes an sql query
      -t, --tables               Lists tables and space used optional filter on  provided table name
      -s, --schema               Display table schema for the provided table name
          --clear                Removes all rows from database
      -c, --conn=VALUE           name of connection string to use (defaults to first found)
      -h, --help                 show this list of options
          
**Basic usage:**

    sql -e "select * from products"
    
**Show All Tables:**

    sql -t [optional-filter]
    
**Show Schema:**

    sql -t [table-name]