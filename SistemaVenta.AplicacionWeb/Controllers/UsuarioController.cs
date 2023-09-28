using Microsoft.AspNetCore.Mvc;


using AutoMapper;
using Newtonsoft.Json;
using SistemaVenta.AplicacionWeb.Models.ViewModels;
using SistemaVenta.AplicacionWeb.Utilidades.Responses;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.Entity;

namespace SistemaVenta.AplicacionWeb.Controllers
{
    public class UsuarioController : Controller
    {

        private readonly IUsuarioService _usuarioService;
        private readonly IRolService _rolService;
        private readonly IMapper _mapper;

        public UsuarioController(IUsuarioService usuarioService, IRolService rolService, IMapper mapper)
        {
            _usuarioService = usuarioService;
            _rolService = rolService;
            _mapper = mapper;
        }





        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> ListaRoles()
        {
            List<VMRol> vmListaRoles = _mapper.Map<List<VMRol>>(await _rolService.Lista());

            return StatusCode(StatusCodes.Status200OK,vmListaRoles);


        }


        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            List<VMUsuario> vmUsuarioLista = _mapper.Map<List<VMUsuario>>(await _usuarioService.Lista());

            return StatusCode(StatusCodes.Status200OK, new { data = vmUsuarioLista });
        }



        [HttpPost]
        public async Task<IActionResult> Crear([FromFormAttribute] IFormFile foto, [FromForm] String modelo)
        {

            GenericResponse<VMUsuario> gResponse = new GenericResponse<VMUsuario>();
            try
            {

                VMUsuario vmUsuario = JsonConvert.DeserializeObject<VMUsuario>(modelo);

                string nombreFoto = "";
                Stream fotoStream = null;

                if (foto != null)
                {

                    string nomber_en_codigo = Guid.NewGuid().ToString("N");    
                    string extension = Path.GetExtension(foto.FileName);
                    nombreFoto = string.Concat(nombreFoto, extension); 
                    
                    fotoStream = foto.OpenReadStream();


                }


                string urlPlantillaCorreo = $"{this.Request.Scheme}://{this.Request.Host}/Plantilla/EnviarClave?correo=[correo]&clave=[clave]";


                Usuario usuario_creado = await _usuarioService.Crear(_mapper.Map<Usuario>(vmUsuario), fotoStream, nombreFoto, urlPlantillaCorreo);


                vmUsuario = _mapper.Map<VMUsuario>(usuario_creado);

                gResponse.Estado = true;
                gResponse.Objeto = vmUsuario;
                



            }
            catch(Exception ex) 
            {

                gResponse.Estado = true;
                gResponse.Mensaje = ex.Message;

            }


            return StatusCode(StatusCodes.Status200OK, gResponse);


        }





        [HttpPut]
        public async Task<IActionResult> Editar ([FromFormAttribute] IFormFile FOTO, [FromForm] String modelo)
        {

            GenericResponse<VMUsuario> gResponse = new GenericResponse<VMUsuario>();
            try
            {

                VMUsuario vmUsuario = JsonConvert.DeserializeObject<VMUsuario>(modelo);

                string nombreFoto = "";
                Stream fotoStream = null;

                if (FOTO != null)
                {

                    string nomber_en_codigo = Guid.NewGuid().ToString("N");
                    string extension = Path.GetExtension(FOTO.FileName);
                    nombreFoto = string.Concat(nombreFoto, extension);

                    fotoStream = FOTO.OpenReadStream();


                }




                Usuario usuario_editado = await _usuarioService.Editar(_mapper.Map<Usuario>(vmUsuario), fotoStream, nombreFoto);


                vmUsuario = _mapper.Map<VMUsuario>(usuario_editado);

                gResponse.Estado = true;
                gResponse.Objeto = vmUsuario;




            }
            catch (Exception ex)
            {

                gResponse.Estado = true;
                gResponse.Mensaje = ex.Message;

            }


            return StatusCode(StatusCodes.Status200OK, gResponse);


        }




        [HttpDelete]
        public async Task<IActionResult> Eliminar (int IdUsuario)
        {

            GenericResponse<string> gResponse = new GenericResponse<string>();

            try
            {



                gResponse.Estado = await  _usuarioService.Eliminar(IdUsuario);





            }
            catch (Exception EX)
            { 

                gResponse.Estado = false;
                gResponse.Mensaje = EX.Message;


            }


            return StatusCode(StatusCodes.Status200OK, gResponse);

        }






    }
}
