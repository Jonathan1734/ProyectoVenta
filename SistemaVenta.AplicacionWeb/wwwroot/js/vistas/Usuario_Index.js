

const MODELO_BASE = {

    idUsuario:0,
    Nombre:"",
    Correo:"",
    Telefono:"",
    IdRol:0,
    EsActivo:1,
    UrlFoto: ""


    


}


let tablaData;

$(document).ready(function () {


    fetch("/Usuario/ListaRoles")
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJson => {
            if (responseJson.length > 0) {
                responseJson.forEach((item) => {

                    $("#cboRol").append(

                        $("<option>").val(item.idRol).text(item.descripcion)

                    )
                })

            }

        })

   

            

     
        



    tablaData = $('#tbdata').DataTable({
        responsive: true,
        "ajax": {
            "url": '/Usuario/Lista',
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "idUsuario","visible": false, "searchable": false },
            {
                "data": "urlFoto", render: function (data) {


                    return `<img style = "height:60px" src=${data} class="rounded mx auto d block"/>`
                }
            },
            { "data": "nombre" },
            { "data": "correo" },
            { "data": "telefono" },
            { "data": "nombreRol" },
            {
                "data": "esActivo", render: function (data) {
                    if (data == 1)
                        return '<span class="badge badge-info">Activo</span>';
                    else
                        return '<span class="badge badge-danger">No Activo</span>';



                }
            },
             {
                "defaultContent": '<button class="btn btn-primary btn-editar btn-sm mr-2"><i class="fas fa-pencil-alt"></i></button>' +
                    '<button class="btn btn-danger btn-eliminar btn-sm"><i class="fas fa-trash-alt"></i></button>',
                "orderable": false,
                "searchable": false,
                "width": "80px"
            }
        ],
        order: [[0, "desc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Usuarios',
                exportOptions: {
                    columns: [2, 3, 4, 5, 6]
                }
            }, 'pageLength'
        ],
        language: {
            url: "https://cdn.datatables.net/plug-ins/1.11.5/i18n/es-ES.json"
        },
    });

})



function mostrarModal(modelo = MODELO_BASE) {
    $("#txtId").val(modelo.idUsuario)
    $("#txtNombre").val(modelo.Nombre)
    $("#txtCorreo").val(modelo.Correo)
    $("#txtTelefono").val(modelo.Telefono)
    $("#cboRol").val(modelo.IdRol == 0 ? $("#cboRol option:first").val() : modelo.IdRol)
    $("#cboEstado").val(modelo.EsActivo)
    $("#txtFoto").val("")
    $("#imgUsuario").attr("src",modelo.UrlFoto)


    $("#modalData").modal("show")



}


$("#btnNuevo").click(function () {
    mostrarModal();


})



$("#btnGuardar").click(function () {


    const inputs = $("input.input-validar").serializeArray();
    const inputs_sin_valor = inputs.filter((item) => item.value.trim() == "")

    if (inputs_sin_valor.length > 0) { 
        const mensaje = `DEBE COMPLETAR EL CAMPO: "${inputs_sin_valor[0].name}"`;
        toastr.warning("", mensaje)
        $(`input[name ="${inputs_sin_valor[0].name}"]`).focus()
        return;
    }


    const modelo = structuredClone(MODELO_BASE);
    modelo["idUsuario"] = parseInt($("#txtId").val())
    modelo["nombre"] = $("#txtNombre").val()
    modelo["Correo"] = $("#txtCorreo").val()
    modelo["Telefono"] = $("#txtTelefono").val()
    modelo["IdRol"] = $("#cboRol").val()
    modelo["EsActivo"] = $("#cboEstado").val()



    const inputFoto = document.getElementById("txtFoto")


    const formData = new FormData();
    formData.append("foto", inputFoto.files[0])
    formData.append("modelo", JSON.stringify(modelo))

    $("#modalData").find("div.modal-content").LoadingOverlay("show");

    if (modelo.idUsuario == 0) {


        fetch("/Usuario/Crear", {


            method: "POST",
            body: formData





        }).then(response => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide");

            return response.ok ? response.json() : Promise.reject(response);
        })
            .then(responseJson => {



                if (responseJson.estado) {

                    tablaData.row.add(responseJson.objeto).draw(false)
                    $("#modalData").modal("hide")
                    swal("Lisot!!!!", "El usario fuecreado", "success")




                } else {
                    swal("Lo sentimos!!!!", responseJson.Mensaje, "error")



                }


            })






    }
    else {

        fetch("/Usuario/Editar", {


            method: "PUT",
            body: formData





        }).then(response => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide");

            return response.ok ? response.json() : Promise.reject(response);
        })
            .then(responseJson => {



                if (responseJson.Estado) {

                    tablaData.row(filaSeleccionada).data(responseJson.Objeto).draw(fasle)
                    filaSeleccionada = null;
                    $("#modalData").modal("hide")
                    swal("Lisot!!!!", "El usario fue modificado", "success")




                } else {
                    swal("Lo sentimos!!!!", responseJson.Mensaje, "error")



                }




            })

    }
    





})



let filaSeleccionada;
$("#tbdata tbody").on("click", ".btn-editar", function () {

    if ($(this).closest("tr").hasClass("child")) {

        filaSeleccionada = $(this).closest("tr").prev();



    } else {

        filaSeleccionada = $(this).closest("tr");

    }


    const data = tablaData.row(filaSeleccionada).data();

    mostrarModal(data);
    
})





$("#tbdata tbody").on("click", ".btn-eliminar", function () {


    let fila;

    if ($(this).closest("tr").hasClass("child")) {

        fila = $(this).closest("tr").prev();



    } else {

        fila = $(this).closest("tr");

    }


    const data = tablaData.row(fila).data();

    swal(
        {
            title: "Estas seguro de eliminar?",
            text: `Eliminar al usuario "${data.nombre}"`,
            type: "warning",
            showCancelButton: true,
            confirmButtonClass: "btn-danger",
            confirmButtonText: "Si, ELIMINAR",
            CancelButton: "No, cancelar",
            closeOnConfirm: false,
            closeOncancel: true




        },
        function (respuesta) {

            if (respuesta) {



                $(".showSweetAlert").LoadingOverlay("show");

                fetch(`/Usuario/Eliminar?IdUsuario=${data.idUsuario}`, {


                    method: "DELETE",
                    





                }).then(response => {
                    $(".showSweetAlert").LoadingOverlay("hide");

                    return response.ok ? response.json() : Promise.reject(response);
                })
                    .then(responseJson => {



                        if (responseJson.estado) {

                            tablaData.row(fila).remove().draw()
                           
                            swal("Lisot!!!!", "El usario fue eliminaod", "success")




                        } else {
                            swal("Lo sentimos!!!!", responseJson.Mensaje, "error")



                        }




                    })





            }




        }




    )

})