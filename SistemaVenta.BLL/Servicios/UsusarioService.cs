using System;
using System.Collections.Generic;
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
    public class UsusarioService: IUsuarioService
    {
        private readonly IGenericRepository<Usuario> _usuarioRepositorio;
        private readonly IMapper _mapper;

        public UsusarioService(IGenericRepository<Usuario> usuarioRepositorio, IMapper mapper)
        {
            _usuarioRepositorio = usuarioRepositorio;
            _mapper = mapper;
        }

        public async Task<List<UsuarioDTO>> Lista()
        {
            try {
                var QuerryUsusario = await _usuarioRepositorio.Consultar();
                var listaUsuarios  = QuerryUsusario.Include(rol => rol.IdRolNavigation);

                return _mapper.Map<List<UsuarioDTO>>(listaUsuarios);
            }
            catch {
                throw;
            }
        }
        public async Task<SesionDTO> ValidarCredenciales(string correo, string clave)
        {
            try {
                var QuerryUsusario = await _usuarioRepositorio.Consultar(u=>
                    u.Correo == correo && u.Clave == clave
                );
                if (QuerryUsusario.FirstOrDefault()==null)
                {
                    throw new TaskCanceledException("El usuario no existe");
                }
                Usuario devolverUsuario = QuerryUsusario.Include(rol => rol.IdRolNavigation).First();
                return _mapper.Map<SesionDTO>(devolverUsuario);
            }
            catch {
                throw;
            }
        }

        public async Task<UsuarioDTO> Crear(UsuarioDTO modelo)
        {
            try
            {
                var usuarioCreado = await _usuarioRepositorio.Crear(_mapper.Map<Usuario>(modelo));

                if (usuarioCreado.IdUsuario == 0)
                {
                    throw new TaskCanceledException("El usuario no se pudo crear");
                }

                var querry = await _usuarioRepositorio.Consultar(u => u.IdUsuario == usuarioCreado.IdUsuario);
                usuarioCreado = querry.Include(rol => rol.IdRolNavigation).First();

                return _mapper.Map<UsuarioDTO>(usuarioCreado);

            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> Editar(UsuarioDTO modelo)
        {
            try
            {
                var usuarioModelo = _mapper.Map<Usuario>(modelo);
                var usuarioEcnontrado = await _usuarioRepositorio.Obtener(u=> u.IdUsuario==modelo.IdUsuario);
                if (usuarioEcnontrado ==  null)
                {
                    throw new TaskCanceledException("El usuario no existe");
                }
                usuarioEcnontrado.NombreCompleto = usuarioModelo.NombreCompleto;
                usuarioEcnontrado.Correo =  usuarioModelo.Correo;
                usuarioEcnontrado.IdRol = usuarioModelo.IdRol;
                usuarioEcnontrado.Clave = usuarioModelo.Clave;
                usuarioEcnontrado.EsActivo = usuarioModelo.EsActivo;

                bool respuesta = await _usuarioRepositorio.Editar(usuarioEcnontrado);
                if (!respuesta)
                {
                    throw new TaskCanceledException("No se puedo agregar");

                }
                return respuesta;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> ELiminar(int id)
        {
            try
            {
                var usuarioEncontrado = await _usuarioRepositorio.Obtener(u => u.IdUsuario == id);
                if (usuarioEncontrado== null)
                {
                    throw new TaskCanceledException("El usuario no existe");

                }
                bool respuesta = await _usuarioRepositorio.Eliminar(usuarioEncontrado);
                if (!respuesta)
                {
                    throw new TaskCanceledException("No se puedo eliminar");

                }
                return respuesta;
            }
            catch
            {
                throw;
            }
        }




    }
}
