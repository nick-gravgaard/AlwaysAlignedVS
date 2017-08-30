using System;
using System.Collections.Generic;
using System.Reflection;

namespace AlwaysAligned
{
	public class WeakDelegate<TDelegate> : IEquatable<TDelegate>
	{
		private readonly WeakReference _targetReference;
		private readonly MethodInfo _method;

		public WeakDelegate(Delegate realDelegate)
		{
			_targetReference = realDelegate.Target != null ? new WeakReference(realDelegate.Target) : null;
			_method = realDelegate.Method;
		}

		public TDelegate GetDelegate()
		{
			return (TDelegate)(object)GetDelegateInternal();
		}

		private Delegate GetDelegateInternal()
		{
			if (_targetReference != null)
			{
				return Delegate.CreateDelegate(typeof(TDelegate), _targetReference.Target, _method);
			}
			return Delegate.CreateDelegate(typeof(TDelegate), _method);
		}

		public bool IsAlive
		{
			get { return _targetReference == null || _targetReference.IsAlive; }
		}

		#region IEquatable<TDelegate> Members

		public bool Equals(TDelegate other)
		{
			Delegate d = (Delegate)(object)other;
			return d != null
				&& d.Target == _targetReference.Target
				&& d.Method.Equals(_method);
		}

		#endregion

		internal void Invoke(params object[] args)
		{
			Delegate handler = GetDelegateInternal();
			handler.DynamicInvoke(args);
		}
	}

	public class WeakEvent<TEventHandler>
	{
		private readonly List<WeakDelegate<TEventHandler>> _handlers;

		public WeakEvent()
		{
			_handlers = new List<WeakDelegate<TEventHandler>>();
		}

		public virtual void AddHandler(TEventHandler handler)
		{
			Delegate d = (Delegate)(object)handler;
			_handlers.Add(new WeakDelegate<TEventHandler>(d));
		}

		public virtual void RemoveHandler(TEventHandler handler)
		{
			// also remove "dead" (garbage collected) handlers
			_handlers.RemoveAll(wd => !wd.IsAlive || wd.Equals(handler));
		}

		public virtual void Raise(object sender, EventArgs e)
		{
			var handlers = _handlers.ToArray();
			foreach (var weakDelegate in handlers)
			{
				if (weakDelegate.IsAlive)
				{
					weakDelegate.Invoke(sender, e);
				}
				else
				{
					_handlers.Remove(weakDelegate);
				}
			}
		}

		protected List<WeakDelegate<TEventHandler>> Handlers
		{
			get { return _handlers; }
		}
	}
}
