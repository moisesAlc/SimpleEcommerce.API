using SimpleEcommerce.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleEcommerce.API.Repositories
{
	
	public class UsuarioRepository : IUsuarioRepository
	{
		static private List<Usuario> _db = new List<Usuario>()
		{
			new Usuario(){ Id=1, Nome="Felipe", Email="felipe2345@gmail.com"},
			new Usuario(){ Id=2, Nome="Marcelo Rodrigues", Email="marcelo2345@gmail.com"},
			new Usuario(){ Id=3, Nome="Jéssica Miranda", Email="jessica.miranda2345@gmail.com"},
		};
		public List<Usuario> Get()
		{
			return _db;
		}

		public Usuario Get(int id)
		{
			return _db.FirstOrDefault(u => u.Id == id);
		}

		public void Insert(Usuario usuario)
		{
			Usuario ultimoUsuario = _db.LastOrDefault();
			if (ultimoUsuario == null)
			{
				usuario.Id = 1;
			}
			else
			{
				usuario.Id = ultimoUsuario.Id;
				usuario.Id++;

			}
			_db.Add(usuario);
		}

		public void Update(Usuario usuario)
		{
			_db.Remove(_db.FirstOrDefault(u => u.Id == usuario.Id));
			_db.Add(usuario);
		}
		public void Delete(int id)
		{
			_db.Remove(_db.FirstOrDefault(u => u.Id == id));
		}
	}
}
