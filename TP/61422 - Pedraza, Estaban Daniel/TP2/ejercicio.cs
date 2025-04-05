// TP2: Sistema de Cuentas Bancarias
//

// Implementar un sistema de cuentas bancarias que permita realizar operaciones como depósitos, retiros, transferencias y pagos.

using System;
using System.Collections.Generic;

abstract class Cuenta
{
    public double Saldo { get; protected set; }
    public int Puntos { get; protected set; }

    public virtual void Depositar(double monto)
    {
        if (monto > 0)
        {
            Saldo += monto;
            SumarPuntos(monto);
        }
    }

    protected abstract void SumarPuntos(double monto);

    public override string ToString()
    {
        return $"Saldo: ${Saldo}, Puntos: {Puntos}";
    }
}

class CuentaOro : Cuenta
{
    protected override void SumarPuntos(double monto)
    {
        Puntos += (int)(monto * 0.1); // 10% del monto en puntos
    }
}

class CuentaPlata : Cuenta
{
    protected override void SumarPuntos(double monto)
    {
        Puntos += (int)(monto * 0.05); // 5% del monto en puntos
    }
}

class CuentaBronce : Cuenta
{
    protected override void SumarPuntos(double monto)
    {
        Puntos += (int)(monto * 0.02); // 2% del monto en puntos
    }
}

class Cliente
{
    public string Nombre { get; private set; }
    public Cuenta Cuenta { get; private set; }

    public Cliente(string nombre, Cuenta cuenta)
    {
        Nombre = nombre;
        Cuenta = cuenta;
    }

    public override string ToString()
    {
        return $"Cliente: {Nombre}, {Cuenta}";
    }
}

class Banco
{
    private List<Cliente> clientes = new List<Cliente>();

    public void AgregarCliente(Cliente cliente)
    {
        clientes.Add(cliente);
    }

    public Cliente BuscarCliente(string nombre)
    {
        return clientes.Find(c => c.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));
    }

    public void MostrarClientes()
    {
        foreach (var cliente in clientes)
        {
            Console.WriteLine(cliente);
        }
    }
}

// --- Código principal para dotnet-script ---
Banco banco = new Banco();
bool salir = false;

while (!salir)
{
    Console.Clear();
    Console.WriteLine("\n=== Bienvenido al sistema bancario de Banco Galicia ===\n");
    Console.WriteLine("1. Agregar cliente");
    Console.WriteLine("2. Depositar a cliente");
    Console.WriteLine("3. Ver datos de cliente");
    Console.WriteLine("4. Mostrar todos los clientes");
    Console.WriteLine("5. Salir");
    Console.Write("Seleccione una opción: ");

    string opcion = Console.ReadLine();

    switch (opcion)
    {
        case "1":
            Console.Write("Ingrese el nombre del cliente: ");
            string nombre = Console.ReadLine();

            Console.WriteLine("Tipo de cuenta (Oro, Plata, Bronce): ");
            string tipo = Console.ReadLine().ToLower();
            Cuenta cuenta = null;
            switch (tipo)
            {
                case "oro":
                    cuenta = new CuentaOro();
                    break;
                case "plata":
                    cuenta = new CuentaPlata();
                    break;
                case "bronce":
                    cuenta = new CuentaBronce();
                    break;
            }

            if (cuenta != null)
            {
                banco.AgregarCliente(new Cliente(nombre, cuenta));
                Console.WriteLine("Cliente agregado exitosamente a Banco Galicia.");
            }
            else
            {
                Console.WriteLine("Tipo de cuenta inválido. Solo se aceptan cuentas Oro, Plata o Bronce.");
            }
            break;

        case "2":
            Console.Write("Ingrese el nombre del cliente: ");
            string nombreDep = Console.ReadLine();
            var clienteDep = banco.BuscarCliente(nombreDep);
            if (clienteDep != null)
            {
                Console.Write("Monto a depositar: $");
                if (double.TryParse(Console.ReadLine(), out double monto))
                {
                    clienteDep.Cuenta.Depositar(monto);
                    Console.WriteLine("Depósito realizado con éxito en Banco Galicia.");
                }
                else
                {
                    Console.WriteLine("Monto inválido. Intente nuevamente.");
                }
            }
            else
            {
                Console.WriteLine("Cliente no encontrado en nuestros registros.");
            }
            break;

        case "3":
            Console.Write("Ingrese el nombre del cliente: ");
            string nombreVer = Console.ReadLine();
            var clienteVer = banco.BuscarCliente(nombreVer);
            if (clienteVer != null)
            {
                Console.WriteLine(clienteVer);
            }
            else
            {
                Console.WriteLine("Cliente no encontrado en Banco Galicia.");
            }
            break;

        case "4":
            Console.WriteLine("\n--- Lista de clientes del Banco Galicia ---\n");
            banco.MostrarClientes();
            break;

        case "5":
            salir = true;
            break;

        default:
            Console.WriteLine("Opción no válida. Intente nuevamente.");
            break;
    }

    if (!salir)
    {
        Console.WriteLine("\nPresione una tecla para continuar...");
        Console.ReadKey();
    }
}

Console.Clear();
Console.WriteLine("Gracias por utilizar el sistema del Banco Galicia. ¡Hasta pronto!");
Console.ReadKey();
