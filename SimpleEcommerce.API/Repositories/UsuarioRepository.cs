using Dapper;
using SimpleEcommerce.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SimpleEcommerce.API.Repositories
{

	public class UsuarioRepository : IUsuarioRepository
	{
		private IDbConnection _connection;

		public UsuarioRepository()
		{
			_connection = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SimpleEcommerce;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
		}

		// não vai fazer JOIN aqui por enquanto, só qdo busca por id
		public List<Usuario> Get()
		{
			//return _connection.Query<Usuario>("SELECT * FROM Usuarios").ToList();

			List<Usuario> usuarios = new List<Usuario>();

			string sql = "SELECT U.*, C.*, EE.*, D.* FROM Usuarios as U " +
				"LEFT JOIN Contatos C ON C.UsuarioId = U.Id " +
				"LEFT JOIN EnderecosEntrega EE On EE.UsuarioId = U.Id " +
				"LEFT JOIN UsuariosDepartamentos UD on UD.UsuarioId = U.Id " +
				"LEFT JOIN Departamentos D on UD.DepartamentoId = D.Id";

			_connection.Query<Usuario, Contato, EnderecoEntrega, Departamento, Usuario>(sql,
				(usuario, contato, enderecoEntrega, departamento) =>
				{
					// verificação do usuário
					if (usuarios.SingleOrDefault(us => us.Id == usuario.Id) == null)
					{
						usuario.Departamentos = new List<Departamento>();
						usuario.EnderecosEntrega = new List<EnderecoEntrega>();
						usuario.Contato = contato;
						usuarios.Add(usuario);
					}
					else
					{
						usuario = usuarios.SingleOrDefault(us => us.Id == usuario.Id);
					}

					// verificação do endereço de entrega
					if (usuario.EnderecosEntrega.SingleOrDefault(a => a.Id == enderecoEntrega.Id) == null)
					{
						 usuario.EnderecosEntrega.Add(enderecoEntrega);
					}

					// verificação do departamento
					if (usuario.Departamentos.SingleOrDefault(a => a.Id == departamento.Id) == null)
					{
						usuario.Departamentos.Add(departamento);
					}

					return usuario; // aqui posso retornar qualquer coisa, o que 
									// importa é o próximo retorno
				});
			return usuarios;
		}

		public Usuario Get(int id)
		{
			//return _connection.Query<Usuario>("SELECT * FROM Usuarios").ToList();

			List<Usuario> usuarios = new List<Usuario>();

			string sql = "SELECT U.*, C.*, EE.*, D.* FROM Usuarios as U " +
				"LEFT JOIN Contatos C ON C.UsuarioId = U.Id " +
				"LEFT JOIN EnderecosEntrega EE On EE.UsuarioId = U.Id " +
				"LEFT JOIN UsuariosDepartamentos UD on UD.UsuarioId = U.Id " +
				"LEFT JOIN Departamentos D on UD.DepartamentoId = D.Id " +
				"WHERE U.Id = @Id";

			_connection.Query<Usuario, Contato, EnderecoEntrega, Departamento, Usuario>(sql,
				(usuario, contato, enderecoEntrega, departamento) =>
				{
					// verificação do usuário
					if (usuarios.SingleOrDefault(us => us.Id == usuario.Id) == null)
					{
						usuario.Departamentos = new List<Departamento>();
						usuario.EnderecosEntrega = new List<EnderecoEntrega>();
						usuario.Contato = contato;
						usuarios.Add(usuario);
					}
					else
					{
						usuario = usuarios.SingleOrDefault(us => us.Id == usuario.Id);
					}

					// verificação do endereço de entrega
					if (usuario.EnderecosEntrega.SingleOrDefault(a => a.Id == enderecoEntrega.Id) == null)
					{
						usuario.EnderecosEntrega.Add(enderecoEntrega);
					}

					// verificação do departamento
					if (usuario.Departamentos.SingleOrDefault(a => a.Id == departamento.Id) == null)
					{
						usuario.Departamentos.Add(departamento);
					}

					return usuario; // aqui posso retornar qualquer coisa, o que 
									// importa é o próximo retorno
				}, new { Id = id});
			
			return usuarios.SingleOrDefault();
		}

		public void Insert(Usuario usuario)
		{
			_connection.Open();

			IDbTransaction transaction = _connection.BeginTransaction();

			try
			{
				string sqlUsuario =
				"INSERT INTO Usuarios (Nome, Email, Sexo, RG, CPF, NomeMae, SituacaoCadastro, DataCadastro) " +
				"VALUES " + "(@Nome, @Email, @Sexo, @RG, @CPF, @NomeMae, @SituacaoCadastro, @DataCadastro)" +
				";" +
				"SELECT CAST( SCOPE_IDENTITY() AS INT);"; // pega o id do usuário inserido
				usuario.Id = _connection.Query<int>(sqlUsuario, usuario, transaction).Single();

				if (usuario.Contato != null)
				{
					usuario.Contato.UsuarioId = usuario.Id;
					string sqlContato =
						"INSERT INTO Contatos (UsuarioId, Telefone, Celular) " +
						"VALUES (@UsuarioId, @Telefone, @Celular)" +
						";" +
						"SELECT CAST( SCOPE_IDENTITY() AS INT);"; // pega o id do contato inserido
					usuario.Contato.Id = _connection.Query<int>(sqlContato, usuario.Contato, transaction).Single();
				}

				if (usuario.EnderecosEntrega != null && usuario.EnderecosEntrega.Count > 0)
				{
					foreach (var enderecoEntrega in usuario.EnderecosEntrega)
					{
						enderecoEntrega.UsuarioId = usuario.Id;
						string sqlEndereco = "INSERT INTO EnderecosEntrega " +
							"( UsuarioId, NomeEndereco, CEP, Estado, Cidade, Bairro, Endereco, Numero, Complemento) " +
							" VALUES " +
							"( @UsuarioId, @NomeEndereco, @CEP, @Estado, @Cidade, @Bairro, @Endereco, @Numero, @Complemento) ;" +
							"SELECT CAST(SCOPE_IDENTITY() AS INT);";
						enderecoEntrega.Id = _connection.Query<int>(sqlEndereco, usuario.EnderecosEntrega, transaction).Single();
					}
				}

				transaction.Commit();
			}
			catch (Exception e)
			{
				try
				{
					transaction.Rollback();
				}
				catch (Exception)
				{
					//@TODO qual é a melhor forma de tratar?
				}

			}
			finally
			{
				_connection.Close();
			}

		}

		public void Update(Usuario usuario)
		{
			_connection.Open();
			var transaction = _connection.BeginTransaction();

			try
			{
				string sqlUsuario =
					"UPDATE Usuarios SET " +
					"Nome = @Nome, Email = @Email, Sexo = @Sexo, RG = @RG, " +
					"CPF = @CPF, NomeMae = @NomeMae, " +
					"SituacaoCadastro = @SituacaoCadastro, " +
					"DataCadastro = @DataCadastro " +
					"WHERE Id = @Id;";
				_connection.Execute(sqlUsuario, usuario, transaction);

				if (usuario.Contato != null)
				{
					string sqlContato =
					"UPDATE Contatos SET " +
					"UsuarioId = @UsuarioId, " +
					"Telefone = @Telefone, " +
					"Celular = @Celular " +
					"WHERE Id = @Id";
					_connection.Execute(sqlContato, usuario.Contato, transaction);

					string sqlDeletarEnderecosEntrega = "DELETE FROM EnderecosEntrega WHERE UsuarioId = @Id";
					_connection.Execute(sqlDeletarEnderecosEntrega, usuario, transaction);

					if (usuario.EnderecosEntrega != null && usuario.EnderecosEntrega.Count > 0)
					{
						foreach (var enderecoEntrega in usuario.EnderecosEntrega)
						{
							enderecoEntrega.UsuarioId = usuario.Id;
							string sqlEndereco = "INSERT INTO EnderecosEntrega " +
								"( UsuarioId, NomeEndereco, CEP, Estado, Cidade, Bairro, Endereco, Numero, Complemento) " +
								" VALUES " +
								"( @UsuarioId, @NomeEndereco, @CEP, @Estado, @Cidade, @Bairro, @Endereco, @Numero, @Complemento) ;" +
								"SELECT CAST(SCOPE_IDENTITY() AS INT);";
							enderecoEntrega.Id = _connection.Query<int>(sqlEndereco, usuario.EnderecosEntrega, transaction).Single();
						}
					}

					transaction.Commit();
				}
			}
			catch (Exception)
			{
				try
				{
					transaction.Rollback();
				}
				catch (Exception)
				{
					//@TODO qual é a melhor forma de tratar?
				}
			}
			finally
			{
				_connection.Close();
			}
		}

		public void Delete(int id)
		{
			_connection.Execute("DELETE FROM Usuarios WHERE Id = @Id", new { Id = id });
		}

	}
}