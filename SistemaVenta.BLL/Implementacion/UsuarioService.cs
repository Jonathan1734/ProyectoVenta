using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity;


namespace SistemaVenta.BLL.Implementacion
{
    public class UsuarioService : IUsuarioService
    {

        private readonly IGenericRepository<Usuario> _repositorio;   
        private readonly IFireBaseService _firebaseService;
        private readonly IUtilidadesService _utilidadesService;
        private readonly ICorreoService _correoService;

        public object UrlPlantillaCorreo { get; private set; }

        public UsuarioService(IGenericRepository<Usuario> repositorio, IFireBaseService firebaseService, IUtilidadesService utilidadesService, ICorreoService correoService)
        {

            _repositorio = repositorio;
            _firebaseService = firebaseService;
            _utilidadesService = utilidadesService;
            _correoService = correoService;


        }
        public async Task<List<Usuario>> Lista()
        {
                IQueryable<Usuario> query = await _repositorio.Consultar();

            return query.Include(r => r.IdRolNavigation).ToList();


        }

        public async Task<Usuario> Crear(Usuario entidad, Stream Foto = null, string NombreFoto = "", string UrlPlantillaCorreo = "")
        {


            Usuario usuario_existe = await _repositorio.Obtener(u => u.Correo == entidad.Correo);

            if(usuario_existe != null)
            {

                throw new TaskCanceledException("El correo ya existe");

            }
            try
            {
                string clave_generada = _utilidadesService.GenerarClave();
                entidad.Clave = _utilidadesService.ConvertirSha256(clave_generada);
                entidad.NombreFoto =NombreFoto;

                if(Foto != null)
                {
                    string urlFoto = await _firebaseService.SubirStorage(Foto, "carpeta_usuario", NombreFoto);
                    entidad.UrlFoto = urlFoto;

                }

                Usuario usuario_creado = await _repositorio.Crear(entidad);

                if(usuario_creado.IdUsuario == 0 )
                    throw new TaskCanceledException("No se pudo crear el usuario");


                if(UrlPlantillaCorreo != "")
                {
                    UrlPlantillaCorreo = UrlPlantillaCorreo.Replace("[correo}", usuario_creado.Correo).Replace("[clave]", clave_generada);



                    string htmlCorreo = "";

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UrlPlantillaCorreo);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();


                    if(response.StatusCode == HttpStatusCode.OK)
                    {
                        using (Stream dataStrean = response.GetResponseStream())
                        {

                            StreamReader readerStream = null;

                            if(response.CharacterSet == null)
                                readerStream = new StreamReader(dataStrean);
                            else 
                                readerStream = new StreamReader(dataStrean,Encoding.GetEncoding(response.CharacterSet));

                                
                            htmlCorreo = readerStream.ReadToEnd();
                            response.Close();
                            readerStream.Close();


                        }



                    }


                    if (htmlCorreo != "")
                        await _correoService.EnviarCorreo(usuario_creado.Correo, "Cuenta_Creada", htmlCorreo);

                }

                IQueryable<Usuario> query = await _repositorio.Consultar(u => u.IdUsuario == usuario_creado.IdUsuario);
                usuario_creado = query.Include(r => r.IdRolNavigation).First();

                return usuario_creado;
            }
            catch(Exception ex) 
            {

                throw;
            }





        }

        public async Task<Usuario> Editar(Usuario entidad, Stream Foto = null, string NombreFoto = "")
        {
            Usuario usuario_existe = await _repositorio.Obtener(u => u.Correo == entidad.Correo && u.IdUsuario != entidad.IdUsuario);

            if (usuario_existe != null)
                throw new TaskCanceledException("El correo ya existe");



            try
            {

                IQueryable <Usuario> queryUsuario = await _repositorio.Consultar(u => u.IdUsuario == entidad.IdUsuario);  
                Usuario usuario_editar = queryUsuario.First();
                usuario_existe.Nombre = entidad.Nombre;
                usuario_existe.Correo = entidad.Correo; 
                usuario_existe.Telefono = entidad.Telefono; 
                usuario_editar.IdRol =   entidad.IdRol;

                if(usuario_editar.NombreFoto == "")
                    usuario_editar.NombreFoto = NombreFoto;

                if (Foto != null)
                {
                    string urlFoto = await _firebaseService.SubirStorage(Foto, "carpeta_usuario", usuario_editar.NombreFoto);
                    usuario_editar.UrlFoto = urlFoto;
                }

                bool respuesta = await _repositorio.Editar(usuario_editar);
                if(respuesta)
                    throw new TaskCanceledException("No se pudo modificar el usuario");


                Usuario usuario_editado = queryUsuario.Include(r => r.IdRolNavigation).First();

                return usuario_editado;


            }
            catch 
            {
                throw;
            }





        }

        public async Task<bool> Eliminar(int IdUsuario)
        {


            try
            {
                Usuario usuario_encontrado = await _repositorio.Obtener(U => U.IdUsuario == IdUsuario);
                if (usuario_encontrado == null)
                    throw new TaskCanceledException("EL usuario no existe");


                string nombreFoto = usuario_encontrado.NombreFoto;
                bool respuesta = await _repositorio.Eliminar(usuario_encontrado);



                if (respuesta)
                    await _firebaseService.EliminarStorage("carpeta_usuario", nombreFoto);
                return true;

            }
            catch 
            {

                throw;
            }
        
        
        
        
        }


        public async Task<Usuario> ObtenerPorCredenciales(string Correo, string clave)
        {

            string clave_encriptada = _utilidadesService.ConvertirSha256(clave);
            Usuario usuario_encontrado = await _repositorio.Obtener(U => U.Correo.Equals(Correo) && U.Clave.Equals(clave_encriptada));


            return usuario_encontrado;
        }

        public async Task<Usuario> ObtenerPorId(int IdUsuario)
        {


            IQueryable<Usuario> query = await _repositorio.Consultar(u => u.IdUsuario == IdUsuario);

            Usuario resultado = query.Include(r => r.IdRolNavigation).FirstOrDefault();


            return resultado;
        
        
        
        }

        public async Task<bool> GuardarPerfil(Usuario entidad)
        {


            try
            {
                Usuario usuario_encontrado = await _repositorio.Obtener(u => u.IdUsuario == entidad.IdUsuario);


                if(usuario_encontrado == null)
                    throw new TaskCanceledException("EL usuario no existe");


                usuario_encontrado.Correo = entidad.Correo;
                usuario_encontrado.Telefono = entidad.Telefono;

                bool respuesta = await _repositorio.Editar(usuario_encontrado);

                return respuesta;

            }
            catch
            
            {
                throw;

            }




        }


        public async Task<bool> CambiarClave(int IdUsuario, string ClaveActual, string ClaveNueva)
        {


            try
            {

                Usuario usuario_encontrado = await _repositorio.Obtener(U => U.IdUsuario == IdUsuario);
                if (usuario_encontrado == null)
                    throw new TaskCanceledException("El usuario no existe :(");

                if(usuario_encontrado.Clave != _utilidadesService.ConvertirSha256(ClaveNueva))
                    throw new TaskCanceledException("La contraseña ingresa como actual no es correcta:(");

                usuario_encontrado.Clave = _utilidadesService.ConvertirSha256(ClaveActual);

                bool respuesta = await _repositorio.Editar(usuario_encontrado);

                return respuesta;   

            }
            catch(Exception ex) { throw; }
            
           


        }

       

      
      

       
        public async Task<bool> RestablecerClave(string Correo, string UrlPlantillaCorreo)
        {
            try
            {
                Usuario usuario_encontrado = await _repositorio.Obtener(U => U.Correo == Correo);
                if (usuario_encontrado == null)
                    throw new TaskCanceledException("No encontramos ningun usuario asociado al correo");



                string clave_generada = _utilidadesService.GenerarClave();
                usuario_encontrado.Clave = _utilidadesService.ConvertirSha256(clave_generada);


                UrlPlantillaCorreo = UrlPlantillaCorreo.Replace("[clave]", clave_generada);


                string htmlCorreo = "";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UrlPlantillaCorreo);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();


                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream dataStrean = response.GetResponseStream())
                    {

                        StreamReader readerStream = null;

                        if (response.CharacterSet == null)
                            readerStream = new StreamReader(dataStrean);
                        else
                            readerStream = new StreamReader(dataStrean, Encoding.GetEncoding(response.CharacterSet));


                        htmlCorreo = readerStream.ReadToEnd();
                        response.Close();
                        readerStream.Close();


                    }



                }

                bool correo_enviado = false;


                if (htmlCorreo != "")
                   correo_enviado = await _correoService.EnviarCorreo(Correo, "Contrase;a Restablecida", htmlCorreo);

                if(correo_enviado)
                    throw new TaskCanceledException("Tenemos problemas Por favor intentalo de nuevo mas tarde");

                bool respuesta = await _repositorio.Editar(usuario_encontrado);
                return respuesta;
            }
            catch  { throw; } 
        }
    }
}
