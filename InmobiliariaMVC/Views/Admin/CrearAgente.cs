using System.ComponentModel.DataAnnotations;
using System.Reflection.Emit;
using static System.Runtime.InteropServices.JavaScript.JSType;

@model InmobiliariaMVC.Models.AgenteCreateViewModel

@{
    ViewData["Title"] = "Crear Agente";
}

< div class= "container-main" >
    < div class= "d-flex justify-content-between align-items-center mb-4" >
        < h1 > Crear Nuevo Agente</h1>
        <a asp-action= "Agentes" class= "btn btn-outline-primary" >
            < i class= "bi bi-arrow-left" ></ i > Volver
        </ a >
    </ div >

    < div class= "card form-card" >
        < form asp - action = "CrearAgente" method = "post" >
            < div asp - validation - summary = "ModelOnly" class= "text-danger mb-3" ></ div >

            < div class= "mb-3" >
                < label asp -for= "NombreCompleto" class= "form-label" >
                    < i class= "bi bi-person" ></ i > Nombre Completo
                </ label >
                < input asp -for= "NombreCompleto" class= "form-control" placeholder = "Ej: Juan Pérez García" />
                < span asp - validation -for= "NombreCompleto" class= "text-danger" ></ span >
            </ div >

            < div class= "mb-3" >
                < label asp -for= "Email" class= "form-label" >
                    < i class= "bi bi-envelope" ></ i > Correo Electrónico
                </ label >
                < input asp -for= "Email" type = "email" class= "form-control" placeholder = "agente@tapiassolution.com" />
                < span asp - validation -for= "Email" class= "text-danger" ></ span >
            </ div >

            < div class= "mb-3" >
                < label asp -for= "Telefono" class= "form-label" >
                    < i class= "bi bi-telephone" ></ i > Teléfono
                </ label >
                < input asp -for= "Telefono" class= "form-control" placeholder = "70012345" />
                < span asp - validation -for= "Telefono" class= "text-danger" ></ span >
            </ div >

            < div class= "mb-3" >
                < label asp -for= "Password" class= "form-label" >
                    < i class= "bi bi-lock" ></ i > Contraseña
                </ label >
                < input asp -for= "Password" type = "password" class= "form-control" placeholder = "Mínimo 6 caracteres" />
                < span asp - validation -for= "Password" class= "text-danger" ></ span >
            </ div >

            < div class= "form-actions" >
                < a asp - action = "Agentes" class= "btn btn-secondary" > Cancelar </ a >
                < button type = "submit" class= "btn btn-primary" >
                    < i class= "bi bi-check-circle" ></ i > Crear Agente
                </ button >
            </ div >
        </ form >
    </ div >
</ div >