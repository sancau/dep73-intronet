namespace DB73.Models.DataAccess
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Collections.Generic;

    using System.Data.Entity;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;

    public static class DataInterface<T>
        where T : class
    {
        #region Config

        //the suffix that will be used in the name of relative properties in project 
        public const string RELATIVE_PROPERTY_SUFFIX = "List";

        #endregion

        #region Push

        //Routing to a certain Push method by input type
        public static void Push(T entity)
        {
            //determine the incoming Type and pass action to a certain access method
            string typeName = typeof(T).Name;
            switch(typeName)
            {
                case "Document":
                    PushDocument(entity as Document);
                    break;
                case "BugTicket":
					PushIndependentTable(entity as T);
					break;
                case "InventoryItem":
                    PushIndependentTable(entity as T);
                    break;
                case "Link":
                    PushLink(entity as Link);
                    break;
                case "Folder":
                    PushFolder(entity as Folder);
                    break;
                case "Message":
                    PushMessage(entity as Message);
                    break;
                case "User":
                    PushUser(entity as User);
                    break;
                default: throw new NotImplementedException();  
            }
        }

		//Generic push for db tables that has no relation with other tables
		private static void PushIndependentTable(T entity)
		{
			using (Context context = new Context())
            {
                PropertyInfo[] properties = entity.GetType().GetProperties();

                PropertyInfo idProperty = properties.Where(prop =>
                                            prop.Name == "ID").FirstOrDefault();

                int id = Convert.ToInt32(idProperty.GetValue(entity, null));

                T db = context.Set<T>().Find(id);

                if (db == null)
                {
                    context.Set<T>().Add(entity);
                    context.SaveChanges();
                }
                else
                {
                    context.Entry(db).CurrentValues.SetValues(entity);
                    context.SaveChanges();
                }
			}
		}

        //Link push
        private static void PushLink(Link link)
        {
            using (Context context = new Context())
            {
                var linkEntity = context.Links.Find(link.ID);

                if (linkEntity == null)
                {
                    #region Prevent extra messages generation

                    foreach (var message in link.MessageList)
                    {
                        context.Entry<Message>(message).State = EntityState.Unchanged;
                    }

                    #endregion

                    context.Links.Add(link);
                    context.SaveChanges();
                }
                else
                {
                    context.Entry(linkEntity).Collection(l => l.MessageList).Load();

                    #region MessageList push

                    foreach (var messageInDB in linkEntity.MessageList)
                    {
                        if (link.MessageList.Find(m => m.ID == messageInDB.ID) == null)
                        {
                            DataInterface<Link, Message>.RemoveRelation(linkEntity.ID, messageInDB.ID);
                        }
                    }

                    foreach (var messageInPush in link.MessageList)
                    {
                        if (linkEntity.MessageList.Find(m => m.ID == messageInPush.ID) == null)
                        {
                            DataInterface<Link, Message>.AddRelation(linkEntity.ID, messageInPush.ID);
                        }
                    }

                    #endregion

                    context.Entry(linkEntity).CurrentValues.SetValues(link);

                    context.SaveChanges();
                }
            }
        }

        //Document push
        private static void PushDocument(Document document)
        {
            using (Context context = new Context())
            {
                var documentEntity = context.Documents.Find(document.ID);

                if (documentEntity == null)
                {
                    #region Prevent extra folders generation

                    foreach (var folder in document.FolderList)
                    {
                        context.Entry<Folder>(folder).State = EntityState.Unchanged;
                    }

                    #endregion

                    context.Documents.Add(document);
                    context.SaveChanges();
                }
                else
                {
                    context.Entry(documentEntity).Collection(d => d.FolderList).Load();

                    #region FolderList push

                    foreach (var folderInDB in documentEntity.FolderList)
                    {
                        if (document.FolderList.Find(f => f.ID == folderInDB.ID) == null)
                        {
                            DataInterface<Document, Folder>.RemoveRelation(documentEntity.ID, folderInDB.ID);
                        }
                    }

                    foreach (var folderInPush in document.FolderList)
                    {
                        if (documentEntity.FolderList.Find(f => f.ID == folderInPush.ID) == null)
                        {
                            DataInterface<Document, Folder>.AddRelation(documentEntity.ID, folderInPush.ID);
                        }
                    }

                    #endregion

                    context.Entry(documentEntity).CurrentValues.SetValues(document);

                    context.SaveChanges();
                }
            }
        }

        //Folder push
        private static void PushFolder(Folder folder)
        {
            using (Context context = new Context())
            {
                var folderEntity = context.Folders.Find(folder.ID);

                if (folderEntity == null)
                {
					#region Prevent extra user generation
				
                    foreach (var user in folder.UserList)
                    {
                        context.Entry<User>(user).State = EntityState.Unchanged;
                    }

                    #endregion

                    #region Prevent extra document generation

                    foreach (var doc in folder.DocumentList)
                    {
                        context.Entry<Document>(doc).State = EntityState.Unchanged;
                    }

                    #endregion

                    context.Folders.Add(folder);
                    context.SaveChanges();
                }
                else
                {
                    context.Entry(folderEntity).Collection(fol => fol.UserList).Load();
                    context.Entry(folderEntity).Collection(fol => fol.DocumentList).Load();

					#region Userlist push
					
                    foreach (var userInDB in folderEntity.UserList)
                    {
                        if (folder.UserList.Find(user => user.ID == userInDB.ID) == null)
                        {
                            DataInterface<Folder, User>.RemoveRelation(folderEntity.ID, userInDB.ID);
                        }
                    }

                    foreach (var userInPush in folder.UserList)
                    {
                        if (folderEntity.UserList.Find(user => user.ID == userInPush.ID) == null)
                        {
                            DataInterface<Folder, User>.AddRelation(folderEntity.ID, userInPush.ID);
                        }
                    }

                    #endregion

                    #region DocumentList push

                    foreach (var documentInDB in folderEntity.DocumentList)
                    {
                        if (folder.DocumentList.Find(d => d.ID == documentInDB.ID) == null)
                        {
                            DataInterface<Folder, Document>.RemoveRelation(folderEntity.ID, documentInDB.ID);
                        }
                    }

                    foreach (var documentInPush in folder.DocumentList)
                    {
                        if (folderEntity.DocumentList.Find(d => d.ID == documentInPush.ID) == null)
                        {
                            DataInterface<Folder, Document>.AddRelation(folderEntity.ID, documentInPush.ID);
                        }
                    }

                    #endregion
                 
                    context.Entry(folderEntity).CurrentValues.SetValues(folder);

                    context.SaveChanges();
                }
            }
        }

        //Message push 
        private static void PushMessage(Message message)
        {
            using (Context context = new Context())
            {
                var messageEntity = context.Messages.Find(message.ID);

                if (messageEntity == null)
                {
                    #region Prevent extra links generation

                    foreach (var link in message.LinkList)
                    {
                        context.Entry<Link>(link).State = EntityState.Unchanged;
                    }

                    #endregion                   

                    #region Prevent extra users generation

                    foreach (var user in message.UserList)
                    {
                        context.Entry<User>(user).State = EntityState.Unchanged;
                    }

                    #endregion

                    context.Messages.Add(message);
                    context.SaveChanges();
                }
                else
                {
                    context.Entry(messageEntity).Collection(m => m.LinkList).Load();
                    context.Entry(messageEntity).Collection(m => m.UserList).Load();

                    #region LinkList push

                    foreach (var linkInDB in messageEntity.LinkList)
					{
						if (message.LinkList.Find(l => l.ID == linkInDB.ID) == null)
						{
							DataInterface<Message, Link>.RemoveRelation(messageEntity.ID, linkInDB.ID);
						}
					}
					
					foreach (var linkInPush in message.LinkList)
                    {
                        if (messageEntity.LinkList.Find(l => l.ID == linkInPush.ID) == null)
                        {
                            DataInterface<Message, Link>.AddRelation(messageEntity.ID, linkInPush.ID);
                        }
                    }
					
					#endregion			                   
                    
                    #region UserList push

                    foreach (var userInDB in messageEntity.UserList)
                    {
                        if (message.UserList.Find(u => u.ID == userInDB.ID) == null)
                        { 
                            DataInterface<Message, User>.RemoveRelation(messageEntity.ID, userInDB.ID);
                        }
                    }

                    foreach (var userInPush in message.UserList)
                    {
                        if (messageEntity.UserList.Find(u => u.ID == userInPush.ID) == null)
                        {
                            DataInterface<Message, User>.AddRelation(messageEntity.ID, userInPush.ID);
                        }
                    }

                    #endregion
                  
                    context.Entry(messageEntity).CurrentValues.SetValues(message);

                    context.SaveChanges();
                }
            }
        }

        //User push
        private static void PushUser(User user)
        {
            using (Context context = new Context())
            {
                var userEntity = context.Users.Find(user.ID);

                if (userEntity == null)
                {
                    #region Prevent extra folders generation

                    foreach (var folder in user.FolderList)
                    {
                        context.Entry<Folder>(folder).State = EntityState.Unchanged;
                    }

                    #endregion

                    #region Prevent extra messages generation

                    foreach (var message in user.MessageList)
                    {
                        context.Entry<Message>(message).State = EntityState.Unchanged;
                    }

                    #endregion

                    context.Users.Add(user);
                    context.SaveChanges();
                }
                else
                {
                    context.Entry(userEntity).Collection(u => u.FolderList).Load();
                    context.Entry(userEntity).Collection(u => u.MessageList).Load();

                    #region FolderList push

                    foreach (var folderInDB in userEntity.FolderList)
                    {
                        if (user.FolderList.Find(f => f.ID == folderInDB.ID) == null)
                        {
                            DataInterface<User, Folder>.RemoveRelation(userEntity.ID, folderInDB.ID);
                        }
                    }

                    foreach (var folderInPush in user.FolderList)
                    {
                        if (userEntity.FolderList.Find(f => f.ID == folderInPush.ID) == null)
                        {
                            DataInterface<User, Folder>.AddRelation(userEntity.ID, folderInPush.ID);
                        }
                    }

                    #endregion

                    #region MessageList push

                    foreach (var messageInDB in userEntity.MessageList)
                    {
                        if (user.MessageList.Find(f => f.ID == messageInDB.ID) == null)
                        {
                            DataInterface<User, Message>.RemoveRelation(userEntity.ID, messageInDB.ID);
                        }
                    }

                    foreach (var messageInPush in user.MessageList)
                    {
                        if (userEntity.MessageList.Find(f => f.ID == messageInPush.ID) == null)
                        {
                            DataInterface<User, Message>.AddRelation(userEntity.ID, messageInPush.ID);
                        }
                    }

                    #endregion                  

                    context.Entry(userEntity).CurrentValues.SetValues(user);

                    context.SaveChanges();
                }
            }
        }

        #endregion

        #region Pull

        //Returns an object of type T from the DB, queried by its ID
        public static T Pull(int id)
        {
            using (var context = new Context())
            {
                var dbSet = context.Set<T>();

                T entityInDb = dbSet.Find(id);

                //get all properties of type T
                PropertyInfo[] properties = entityInDb.GetType().GetProperties();

                //get list of relative properties (based on RELATIVE_PROPERTY_SUFFIX)
                List<PropertyInfo> relativePropertiesList = new List<PropertyInfo>();

                foreach (var prop in properties)
                {
                    if (prop.Name.Contains(RELATIVE_PROPERTY_SUFFIX) && prop.Name.Length != 4)
                    {
                        relativePropertiesList.Add(prop);
                    }                  
                }

                foreach (var property in relativePropertiesList)
                {
                    //load every relative property to our object from DB
                    context.Entry(entityInDb).Collection(property.Name).Load();
                }

                return entityInDb;
            }
        }
        
        #endregion

        #region List
        //Returns list of entities for T type in the DB
        public static List<T> List
        {
            get
            {
                using (Context context = new Context())
                {
                    var dbSet = context.Set<T>();
                    return dbSet.ToList();
                }
            }
        }

        #endregion

        #region Delete

        //Removes certain object of T type from DB, queried by its ID
        public static void Delete(int id)
        {
            using (Context context = new Context())
            {
                var dbSet = context.Set<T>();

                T entityToRemove = dbSet.Find(id);          

                if (entityToRemove != null)
                {
                    dbSet.Remove(entityToRemove);

                    context.SaveChanges();
                }
            }
        }
        
        #endregion
    }

    public static class DataInterface<T, Y>
        where T : class
        where Y : class
    {
        #region AddRelation

        //Adds relative object of type Y to relative list of type Y, queried by IDs
        public static void AddRelation(int parentEntityID,
                                                int relativeEntityID)
        {
            using (Context context = new Context())
            {
                System.Data.Entity.DbSet<T> parentDbSet = context.Set<T>();
                System.Data.Entity.DbSet<Y> relativeDbSet = context.Set<Y>();

                T parentEntity = parentDbSet.Find(parentEntityID);
                Y relativeEntity = relativeDbSet.Find(relativeEntityID);

                Type relativeType = typeof(Y);

                string relativePropertyName =
                    GetRelativePropertyName(relativeType);

                PropertyInfo relativeProperty = parentEntity.GetType()
                    .GetProperty(relativePropertyName);

                IList<Y> list = relativeProperty.
                            GetValue(parentEntity, null) as IList<Y>;

                list.Add(relativeEntity);

                relativeProperty.SetValue(parentEntity, list, null);

                context.SaveChanges();
            }
        }

        #endregion

        #region RemoveRelation

        //Removes relative object of type Y from the relative list of type T, queried by IDs
        public static void RemoveRelation(int parentEntityID,
                                                int relativeEntityID)
        {
            using (Context context = new Context())
            {
                System.Data.Entity.DbSet<T> parentDbSet = context.Set<T>();
                System.Data.Entity.DbSet<Y> relativeDbSet = context.Set<Y>();

                T parentEntity = parentDbSet.Find(parentEntityID);
                Y relativeEntity = relativeDbSet.Find(relativeEntityID);

                Type relativeType = typeof(Y);

                string relativePropertyName =
                    GetRelativePropertyName(relativeType);

                PropertyInfo relativeProperty = parentEntity.GetType()
                    .GetProperty(relativePropertyName);

                IList<Y> list = relativeProperty.
                            GetValue(parentEntity, null) as IList<Y>;

                list.Remove(relativeEntity);

                relativeProperty.SetValue(parentEntity, list, null);

                context.SaveChanges();
            }
        }

        #endregion

        #region GetRelativePropertyName

        //Service method for DataInterface<T, Y>
        private static string GetRelativePropertyName(Type type)
        {
            string typeName = type.Name;

            string relativePropertyName = typeName + DataInterface<object>.RELATIVE_PROPERTY_SUFFIX;

            return relativePropertyName;
        }

        #endregion
    }

    public static class ServerAccess
    {
        #region GetCurrentServerTime
        public static DateTime CurrentTime
        {
            get
            {
                using (Context context = new Context())
                {
                    try
                    {
                        DateTime serverTime;

                        ObjectQuery<DateTime> query = ((IObjectContextAdapter)context).
                            ObjectContext.CreateQuery<DateTime>("CurrentDateTime() ");

                        serverTime = query.AsEnumerable().First();

                        return serverTime;
                    }
                    catch
                    {
                        return DateTime.MinValue;
                    }
                }
            }
        }
        #endregion

        #region GetCurrentConnectionString

        public static string GetCurrentConnectionString()
        {
            using (Context context = new Context())
            {
                return context.Database.Connection.ConnectionString;
            }
        }

        #endregion
    }
}

