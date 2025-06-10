using EmberaEngine.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Core
{

    public interface IAssetReference<T> where T : class
    {
        public bool isLoaded { get; }
        public T value { get; }
        public event Action<T> OnLoad;

        public void SetValue(T value);
        public void Unload();
    
    }

    public class TextureReference : IAssetReference<Texture>
    {
        public bool isLoaded => _loaded;

        public Texture value => _value;

        bool _loaded;
        Texture _value;

        public event Action<Texture> OnLoad = (Texture value) => { };

        public void SetValue(Texture value)
        {
            this._value = value;
            _loaded = true;

            OnLoad.Invoke(_value);
        }

        public void Unload()
        {
            throw new NotImplementedException();
        }
    }
}
