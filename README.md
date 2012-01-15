Simple commands to query your database with NetBash. ConnectionString name can be set with --conn=[yourdb] if it isnt set it will default to the first one found.
    
    Usage: sql [OPTIONS] [QUERY]
    Execute sql on your database using NetBash
    
    Options:
      -e, --execute              Executes an sql query
      -i, --info                 Shows database information
      -t, --tables               Lists tables and space used
          --clear                Removes all rows from database
      -c, --conn=VALUE           name of connection string to use (defaults to first found)
      -h, --help                 show this list of options
      
Example usage:

    sql -e "select * from products"