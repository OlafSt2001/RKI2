using System.Windows.Input;

namespace RKI2.ViewModels
{
    public class LoadDataCommand : ICommand
    {
        //Da wir die Implementierung von CanExecute() und Execute() gern von außen setzen wollen,
        //müssen wir hier etwas Fußarbeit leisten.
        private readonly Action<object?>? _Execute;
        private readonly Predicate<object?>? _canExecute;

        //Beim Constructor-Aufruf geben wir die beiden Methoden an, die bei Execute/CanExecute
        //ausgeführt werden sollen
        //public LoadDataCommand(Action<object> Execute, Predicate<object> canExecute)
        //{
        //    this.Execute = Execute;
        //    this.canExecute = canExecute;
        //}
        //Modernes C#:
        public LoadDataCommand(Action<object?> execute, Predicate<object?>? canExecute) =>
            (this._Execute, this._canExecute) = (execute, canExecute);

        //Komfort: Wenn wir kein CanExecute brauchen, machen wir einen Bonus-Konstruktor, wo
        //wir eben kein canExecute-Methödchen mitgeben müssen
        public LoadDataCommand(Action<object?> execute) : this(execute, null)
        {

        }

        //Und zu guter letzt müssen wir ja auch ein CanExecute mal auslösen können, was ja auch von außen
        //passieren muss (das ViewModel soll das machen !). Also geben wir diese Möglichkeit:
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        #region Implementierung ICommand
        //Kann das Command überhaupt ausgeführt werden = False => NEIN
        //Wenn kein Handler für CanExecute => True, weil geht dann ja immer
        public bool CanExecute(object? parameter)
        {
           return _canExecute?.Invoke(parameter) ?? true;
        }

        //Der Payload, wenn das Command ausgeführt werden soll
        public void Execute(object? parameter)
        {
            //Wir führen die Methode aus, die im Execute-Feld eingetragen wurde.
            //Wenn nix drin (Execute == null), dann machen wir nix. Dafür sorgt das Fragezeichen.
            _Execute?.Invoke(parameter);
        }

        public event EventHandler? CanExecuteChanged;
        #endregion

    }
}
