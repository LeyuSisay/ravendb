﻿// -----------------------------------------------------------------------
//  <copyright file="From40To41.cs" company="Hibernating Rhinos LTD">
//      Copyright (c) Hibernating Rhinos LTD. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Isam.Esent.Interop;
using Raven.Database.Impl;
using Raven.Abstractions.Extensions;
using Raven.Database.Storage;
using Raven.Storage.Esent.StorageActions;

namespace Raven.Storage.Esent.SchemaUpdates.Updates
{
	public class From42To43 : ISchemaUpdate
	{
		public string FromSchemaVersion { get { return "4.2"; } }

		public void Init(IUuidGenerator generator)
		{

		}

		public void Update(Session session, JET_DBID dbid)
		{
			var tableAndColumns = new[]
			{
				new {Table = "indexes_stats", Column = "last_indexed_timestamp"},
				new {Table = "indexes_stats_reduce", Column = "last_reduced_timestamp"},
				new {Table = "transactions", Column = "timeout"},
				new {Table = "documents", Column = "last_modified"},
				new {Table = "documents_modified_by_transaction", Column = "last_modified"},
				new {Table = "scheduled_reductions", Column = "timestamp"},
				new {Table = "scheduled_reductions", Column = "timestamp"},
				new {Table = "mapped_results", Column = "timestamp"},
				new {Table = "reduce_results", Column = "timestamp"},
				new {Table = "tasks", Column = "added_at"},
			};

			foreach (var tableAndColumn in tableAndColumns)
			{
				var tx = new Transaction(session);
				try
				{
					int rows = 0;
					using (var table = new Table(session, dbid, tableAndColumn.Table, OpenTableGrbit.None))
					{
						Api.MoveBeforeFirst(session, table);
						while (Api.TryMoveNext(session, table))
						{
							var columnid = Api.GetTableColumnid(session, table, tableAndColumn.Column);
							using (var update = new Update(session, table, JET_prep.Replace))
							{
								var bytes = Api.RetrieveColumn(session, table, columnid);
								var date = DateTime.FromOADate(BitConverter.ToDouble(bytes, 0));
								Api.SetColumn(session, table, columnid, date.ToBinary());
								update.Save();
							}
							if (rows ++ <= 1000) 
								continue;
							// pulsing transaction
							tx.Commit(CommitTransactionGrbit.LazyFlush);
							tx.Dispose();
							tx = new Transaction(session);
						}
					}
					tx.Commit(CommitTransactionGrbit.LazyFlush);
				}
				finally
				{
					tx.Dispose();
				}
			}

			SchemaCreator.UpdateVersion(session, dbid, "4.3");
		}
	}
}