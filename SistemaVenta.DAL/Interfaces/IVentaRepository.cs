using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SistemaVenta.Entity;

namespace SistemaVenta.DAL.Interfaces
{
    public interface IVentaRepository: IGenericRepository<Venta>   
    {
        Task<Venta> Registar(Venta entidad);

        Task<List<DetalleVenta>> Reporte(DateTime FechaInicio, DateTime FechaFin);


    }
}
