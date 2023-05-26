using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoMapper;
using SistemaVenta.BLL.Servicios.Contrato;
using SistemaVenta.DAL.Repositorios.Contrato;
using SistemaVenta.DTO;
using SistemaVenta.Model;

namespace SistemaVenta.BLL.Servicios
{
    public class DashboardService: IDashboardSevice
    {
        private readonly IVentaRepository _ventaRepositorio;
        private readonly IGenericRepository<Producto> _productoRepositorio;
        private readonly IMapper _mapper;

        public DashboardService(IVentaRepository ventaRepositorio, IGenericRepository<Producto> productoRepositorio, IMapper mapper)
        {
            _ventaRepositorio = ventaRepositorio;
            _productoRepositorio = productoRepositorio;
            _mapper = mapper;
        }

        private IQueryable<Venta> retornarVentas(IQueryable<Venta> tablaVenta, int restartCantidadDias)
        {
            DateTime? ultimaFecha = tablaVenta.OrderByDescending(v=> v.FechaRegistro).Select(v=>v.FechaRegistro).First();
            ultimaFecha = ultimaFecha.Value.AddDays(restartCantidadDias);

            return tablaVenta.Where(v => v.FechaRegistro.Value.Date >= ultimaFecha.Value.Date);
        }

        private async Task<int> TotalVentasUltimaSemana()
        {
            int total = 0;
            IQueryable<Venta> _VentaQuery = await _ventaRepositorio.Consultar();
            if (_VentaQuery.Count() > 0)
            {
                var tablaVenta = retornarVentas(_VentaQuery,-7);
                total = tablaVenta.Count();
            }
            return total;
        }

        private async Task<string> TotalIngresosUltimaSemana()
        {
            decimal resultado = 0;
            IQueryable<Venta> _VentaQuery = await _ventaRepositorio.Consultar();

            if (_VentaQuery.Count() > 0)
            {
                var tablaVenta = retornarVentas(_VentaQuery, -7);
                resultado = tablaVenta.Select(v=> v.Total).Sum(v=>v.Value);
            }
            return Convert.ToString(resultado, new CultureInfo("es-MX"));
        }

        private async Task<int> totalProductos()
        {
            IQueryable<Producto> _prodcutoQuery = await _productoRepositorio.Consultar();
            int total = _prodcutoQuery.Count();
            return total;

        }

        private async Task<Dictionary<string,int>> ventasUltimaSemana()
        {
            Dictionary<string,int> resultado = new Dictionary<string,int>();
            IQueryable<Venta> _VentaQuery = await _ventaRepositorio.Consultar();
            if (_VentaQuery.Count() > 0)
            {
                var tablaVenta = retornarVentas(_VentaQuery, -7);
                resultado = tablaVenta
                    .GroupBy(v => v.FechaRegistro.Value.Date)
                    .OrderBy(g => g.Key)
                    .Select(dv => new { fecha = dv.Key.ToString("dd/MM/yyyy"), total = dv.Count() })
                    .ToDictionary(keySelector: r => r.fecha, elementSelector: r => r.total);
            }
            return resultado;

        }

        public async Task<DashboardDTO> Resumen()
        {
            DashboardDTO vmDashboard = new DashboardDTO();
            try {
                vmDashboard.TotalVentas = await TotalVentasUltimaSemana();
                vmDashboard.TotalIngresos = await TotalIngresosUltimaSemana();
                vmDashboard.TotalProductos = await totalProductos();

                List<VentasSemanaDTO> listaVentaSemana = new List<VentasSemanaDTO> ();
                foreach (KeyValuePair<string,int> item in await ventasUltimaSemana())
                {
                    listaVentaSemana.Add(new VentasSemanaDTO() { 
                        Fecha = item.Key,
                        Total = item.Value
                    });
                }
                vmDashboard.VentasUltimaSemana = listaVentaSemana;
            }
            catch {
                throw;
            }
            return vmDashboard;
        }
    }
}
