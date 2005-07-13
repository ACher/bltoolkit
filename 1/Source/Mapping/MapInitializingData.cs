/*
 * File:    MapInitializingData.cs
 * Created: 10/12/2004
 * Author:  Igor Tkachev
 *          mailto:it@rsdn.ru
 */
using System;

namespace Rsdn.Framework.Data.Mapping
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class MapInitializingData
	{
		/// <summary>
		/// 
		/// </summary>
		public MapInitializingData()
		{
		}

		internal MapInitializingData(
			IMapDataSource dataSource,
			object         sourceData,
			object[]       parameters)
		{
			_dataSource = dataSource;
			_sourceData = sourceData;
			_parameters = parameters;
		}

		internal void SetSourceData(object data)
		{
			_sourceData = data;
		}

		private IMapDataSource _dataSource;
		/// <summary>
		/// 
		/// </summary>
		public  IMapDataSource  DataSource
		{
			get { return _dataSource; }
		}

		private object _sourceData;
		/// <summary>
		/// 
		/// </summary>
		public 	object  SourceData
		{
			get { return _sourceData; }
		}

		private object [] _parameters;
		/// <summary>
		/// 
		/// </summary>
		public  object []  Parameters
		{
			get { return _parameters; }
		}

		private object [] _memberParameters;
		/// <summary>
		/// 
		/// </summary>
		public  object []  MemberParameters
		{
			get { return _memberParameters;  }
			set { _memberParameters = value; }
		}

		private bool _stopMapping;
		/// <summary>
		/// 
		/// </summary>
		public  bool  StopMapping
		{
			get { return _stopMapping;  }
			set { _stopMapping = value; }
		}
	}
}