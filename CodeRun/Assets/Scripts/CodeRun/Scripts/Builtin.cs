using UnityEngine;
using System.Threading.Tasks;

namespace CodeRun
{
    public class Builtin
    {
        CodeComponent code;
        public Builtin(Environment env, CodeComponent code)
        {
            this.code = code;
            env.Add("print", Print);
            env.Add("delay", Delay);
        }

        public void Print(CodeObject var) => Debug.Log(var.String);

        public async void Delay(CodeObject var)
        {
            if (!var.IsNumber)
            {
                return;
            }

            code.Pause();
            var milli = 1000*var.n;
            await Task.Delay((int)milli);
            code.Play();
        }
    }
}