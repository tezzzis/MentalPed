using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveSystem : MonoBehaviour
{
    public void GuardarPartida()
    {
        DatosGuardado datos = new DatosGuardado();

        // Guardar monedas
       // datos.monedas = ShopManager.Instance.monedas;

        // Guardar inventario
        foreach (var slot in InventoryManager.Instance.slots)
        {
            datos.itemsInventario.Add(slot.data.itemName);
        }

        // Guardar muebles colocados
        foreach (var mueble in GridManager.Instance.mueblesColocados)
        {
            datos.mueblesColocados.Add(new DatosMueble()
            {
                nombre = mueble.Key.itemName,
                posicion = mueble.Value.transform.position
            });
        }

        // Escribir archivo
        BinaryFormatter formatter = new BinaryFormatter();
        string ruta = Application.persistentDataPath + "/save.dat";
        FileStream stream = new FileStream(ruta, FileMode.Create);
        formatter.Serialize(stream, datos);
        stream.Close();
    }

    public void CargarPartida()
    {
        string ruta = Application.persistentDataPath + "/save.dat";
        if (File.Exists(ruta))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(ruta, FileMode.Open);
            DatosGuardado datos = formatter.Deserialize(stream) as DatosGuardado;
            stream.Close();

            // Cargar monedas
            //ShopManager.Instance.monedas = datos.monedas;
            //ShopManager.Instance.ActualizarUI();

            // Cargar inventario
            //InventoryManager.Instance.ResetInventario();
            foreach (string itemNombre in datos.itemsInventario)
            {
                FurnitureData data = Resources.Load<FurnitureData>(itemNombre);
                InventoryManager.Instance.AgregarAlInventario(data);
            }

            // Cargar muebles
            GridManager.Instance.ResetGrid();
            foreach (DatosMueble mueble in datos.mueblesColocados)
            {
                FurnitureData data = Resources.Load<FurnitureData>(mueble.nombre);
                GameObject obj = Instantiate(data.prefab, mueble.posicion, Quaternion.identity);
                GridManager.Instance.MarkAreaOccupied(mueble.posicion, data.size, obj, true);
            }
        }
    }
}

[System.Serializable]
public class DatosGuardado
{
    public int monedas;
    public List<string> itemsInventario = new List<string>();
    public List<DatosMueble> mueblesColocados = new List<DatosMueble>();
}

[System.Serializable]
public class DatosMueble
{
    public string nombre;
    public Vector3 posicion;
}