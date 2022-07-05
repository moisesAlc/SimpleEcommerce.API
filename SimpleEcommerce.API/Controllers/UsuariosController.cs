using Microsoft.AspNetCore.Mvc;
using SimpleEcommerce.API.Repositories;
using SimpleEcommerce.API.Models;

namespace SimpleEcommerce.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UsuariosController : ControllerBase
	{
		private IUsuarioRepository _repository;
		public UsuariosController()
		{
			_repository = new UsuarioRepository();
		}

		/*
		 * CRUD
		 * GET - Obter a lista de usuários.
		 * GET - Obter o usuário passando o id.
		 * POST - Cadastrar um usuário.
		 * PUT - Atualizar um usuário.
		 * DELETE - Remover um usuário.
		 * 
		 * www.minhaapi.com.br/api/Usuarios
		 */

		[HttpGet]
		public IActionResult Get()
		{
			return Ok(_repository.Get());
		}

		[HttpGet("{id}")]
		public IActionResult Get(int id)
		{
			Usuario usuario = _repository.Get(id);
			if (usuario == null)
			{
				return NotFound("Usuário não encontrado");
			}
			return Ok(_repository.Get(id));
		}

		[HttpPost]
		public IActionResult Insert([FromBody]Usuario usuario)
		{
			_repository.Insert(usuario);
			return Ok(usuario);
		}

		[HttpPut]
		public IActionResult Update([FromBody] Usuario usuario)
		{
			_repository.Update(usuario);
			return Ok(usuario);
		}

		[HttpDelete("{id}")]
		public IActionResult Delete(int id)
		{
			_repository.Delete(id);
			return Ok();
		}
	}
}
