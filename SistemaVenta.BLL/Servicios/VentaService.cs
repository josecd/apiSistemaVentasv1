using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SistemaVenta.BLL.Servicios.Contrato;
using SistemaVenta.DAL.Repositorios.Contrato;
using SistemaVenta.DTO;
using SistemaVenta.Model;

namespace SistemaVenta.BLL.Servicios
{
    public  class VentaService: IVentaService
    {
        private readonly IVentaRepository _ventaRepositorio;
        private readonly IGenericRepository<DetalleVenta> _detalleVentaRepositorio;
        private readonly IMapper _mapper;

        public VentaService(IVentaRepository ventaRepository, IGenericRepository<DetalleVenta> detalleVentaRepositorio, IMapper mapper)
        {
            _ventaRepositorio = ventaRepository;
            _detalleVentaRepositorio = detalleVentaRepositorio;
            _mapper = mapper;
        }

        public async Task<List<VentaDTO>> Historial(string buscarPor, string numeroVentas, string fechaInicio, string fechaFin)
        {
            IQueryable<Venta> query = await _ventaRepositorio.Consultar();
            var ListaResultado = new List<Venta>();
            try {

                if (buscarPor == "fecha")
                {
                    DateTime fehc_inicio = DateTime.ParseExact(fechaInicio, "dd/MM/yyyy", new CultureInfo("es-MX"));
                    DateTime fehc_fin = DateTime.ParseExact(fechaFin, "dd/MM/yyyy", new CultureInfo("es-MX"));
                    Console.WriteLine(fehc_inicio);
                    Console.WriteLine(fehc_fin);
                    Console.WriteLine("______________________________________");

                    ListaResultado = await query.Where(v =>
                        v.FechaRegistro.Value.Date >= fehc_inicio.Date &&
                        v.FechaRegistro.Value.Date <= fehc_fin.Date
                    ).Include(dv => dv.DetalleVenta)
                    .ThenInclude(p => p.IdProductoNavigation)
                    .ToListAsync();
                }
                else
                {
                    ListaResultado = await query.Where(v => v.NumeroDocumento == numeroVentas
                   ).Include(dv => dv.DetalleVenta)
                   .ThenInclude(p => p.IdProductoNavigation)
                   .ToListAsync();
                }


            }
            catch {
                throw;
            }
            return _mapper.Map<List<VentaDTO>>(ListaResultado);
        }

        public async Task<VentaDTO> Registrar(VentaDTO modelo)
        {
            try {
                var ventaGenerada = await _ventaRepositorio.Registrar(_mapper.Map<Venta>(modelo));
                if (ventaGenerada.IdVenta == 0)
                {
                    throw new TaskCanceledException("No se pudo crear");
                }
                return _mapper.Map<VentaDTO>(ventaGenerada);
            } catch {
                    throw ;

            }

        }

        public async Task<List<ReporteDTO>> Reporte(string fechaInicio, string fechaFin)
        {
            IQueryable<DetalleVenta> query = await _detalleVentaRepositorio.Consultar();
            var ListaResultrado = new List<DetalleVenta>();
            try {
                DateTime fehc_inicio = DateTime.ParseExact(fechaInicio, "dd/MM/yyyy", new CultureInfo("es-MX"));
                DateTime fehc_fin = DateTime.ParseExact(fechaFin, "dd/MM/yyyy", new CultureInfo("es-MX"));
                ListaResultrado = await query
                    .Include(p=> p.IdVentaNavigation)
                    .Include(v => v.IdVentaNavigation)
                    .Where(dv => 
                        dv.IdVentaNavigation.FechaRegistro.Value.Date >= fehc_inicio.Date &&
                        dv.IdVentaNavigation.FechaRegistro.Value.Date >= fehc_fin.Date
                    ).ToListAsync();
            }
            catch {
                throw;
            }
            return _mapper.Map<List<ReporteDTO>>(ListaResultrado);
        }
    }
}
