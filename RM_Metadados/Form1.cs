// Blender → RM Metadados → Unity
// Um ponto importante: a imagem que aparece no PictureBox é carregada 
// como Bitmap apenas para visualização. O arquivo que você salva no botão 
// Download não é esse Bitmap; são os bytes originais já limpos. Portanto, 
// o PictureBox não interfere na qualidade do arquivo salvo.

//ORIGINAL
//                       │
//                       ▼
//              ┌─────────────────┐
//              │  textura.png    │
//              │                 │
//              │ pixels          │
//              │ Alpha           │
//              │ transparência   │
//              │ resolução       │
//              │ compressão      │
//              │ metadados       │
//              └────────┬────────┘
//                       │
//                 REMOVER METADADOS
//                       │
//                       ▼
//              ┌─────────────────┐
//              │ textura_limpa   │
//              │                 │
//              │ pixels          │ ← mantém
//              │ Alpha           │ ← mantém
//              │ transparência   │ ← mantém
//              │ resolução       │ ← mantém
//              │ compressão      │ ← mantém
//              │ metadados       │ ← removidos
//              └─────────────────┘
//                       │
//                       ▼
//                     UNITY


using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MetadataExtractor;

namespace RM_Metadados
{
    public partial class Form1 : Form
    {
        //private string caminhoImagemOriginal = null;
        //private byte[] imagemSemMetadados = null;//// Arquivo resultante depois da remoção dos metadados.
        //private string extensaoImagem = null;

        private string? caminhoImagemOriginal = null;
        private byte[]? imagemSemMetadados = null;
        private string? extensaoImagem = null;

        public Form1()
        {
            InitializeComponent();

            // ============================================================
            // JANELA
            // ============================================================

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = true;

            this.ClientSize = new Size(660, 600);

            // ============================================================
            // BOTÕES
            // ============================================================

            btnRemoverMetadados.Enabled = false;
            btnDownload.Enabled = false;
            btnDownloadMetadados.Enabled = false;

            // ============================================================
            // TABELA
            // ============================================================

            ConfigurarTabelaMetadados();

            // ============================================================
            // LAYOUT
            // ============================================================

            ConfigurarLayoutFixo();
        }

        // ============================================================
        // CONFIGURAR TABELA
        // ============================================================

        private void ConfigurarTabelaMetadados()
        {
            dataGridViewMetadados.Columns.Clear();
            dataGridViewMetadados.Rows.Clear();

            dataGridViewMetadados.Columns.Add(
                "Grupo",
                "Grupo");

            dataGridViewMetadados.Columns.Add(
                "Metadado",
                "Metadado");

            dataGridViewMetadados.Columns.Add(
                "ID",
                "ID");

            dataGridViewMetadados.Columns.Add(
                "Valor",
                "Valor");

            dataGridViewMetadados.ReadOnly = true;

            dataGridViewMetadados.AllowUserToAddRows = false;
            dataGridViewMetadados.AllowUserToDeleteRows = false;

            dataGridViewMetadados.RowHeadersVisible = false;

            dataGridViewMetadados.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            dataGridViewMetadados.MultiSelect = false;

            dataGridViewMetadados.AutoSizeRowsMode =
                DataGridViewAutoSizeRowsMode.AllCells;

            dataGridViewMetadados.DefaultCellStyle.WrapMode =
                DataGridViewTriState.True;

            dataGridViewMetadados.Columns["Grupo"].Width = 125;
            dataGridViewMetadados.Columns["Metadado"].Width = 165;
            dataGridViewMetadados.Columns["ID"].Width = 65;

            dataGridViewMetadados.Columns["Valor"].AutoSizeMode =
                DataGridViewAutoSizeColumnMode.Fill;
        }

        // ============================================================
        // SELECIONAR IMAGEM
        // ============================================================

        private void btnSelecionar_Click(
            object sender,
            EventArgs e)
        {
            using (OpenFileDialog abrirImagem =
                   new OpenFileDialog())
            {
                abrirImagem.Title =
                    "Selecionar imagem";

                abrirImagem.Filter =
                    "Imagens|*.png;*.jpg;*.jpeg|" +
                    "PNG|*.png|" +
                    "JPEG|*.jpg;*.jpeg";

                if (abrirImagem.ShowDialog() !=
                    DialogResult.OK)
                {
                    return;
                }

                try
                {
                    caminhoImagemOriginal =
                        abrirImagem.FileName;

                    extensaoImagem =
                        Path.GetExtension(
                            caminhoImagemOriginal)
                        .ToLowerInvariant();

                    imagemSemMetadados = null;

                    // ------------------------------------------------
                    // LIBERAR IMAGEM ANTERIOR
                    // ------------------------------------------------

                    if (pictureBox1.Image != null)
                    {
                        pictureBox1.Image.Dispose();
                        pictureBox1.Image = null;
                    }

                    // ------------------------------------------------
                    // LIMPAR TABELA
                    // ------------------------------------------------

                    dataGridViewMetadados.Rows.Clear();

                    // ------------------------------------------------
                    // CARREGAR IMAGEM
                    // ------------------------------------------------

                    using (Image imagem =
                           Image.FromFile(
                               caminhoImagemOriginal))
                    {
                        pictureBox1.Image =
                            new Bitmap(imagem);
                    }

                    // ------------------------------------------------
                    // MOSTRAR METADADOS
                    // ------------------------------------------------

                    ExibirMetadados(
                        caminhoImagemOriginal);

                    // ------------------------------------------------
                    // BOTÕES
                    // ------------------------------------------------

                    btnRemoverMetadados.Enabled = true;

                    btnDownloadMetadados.Enabled = true;

                    btnDownload.Enabled = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Não foi possível abrir a imagem.\n\n" +
                        ex.Message,
                        "Erro",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        // ============================================================
        // LER METADADOS
        // ============================================================

        private IReadOnlyList<MetadataExtractor.Directory>
            LerMetadados(string caminho)
        {
            return ImageMetadataReader.ReadMetadata(
                caminho);
        }

        // ============================================================
        // EXIBIR METADADOS
        // ============================================================

        private void ExibirMetadados(
            string caminho)
        {
            dataGridViewMetadados.Rows.Clear();

            var diretorios =
                LerMetadados(caminho);

            int quantidade = 0;

            foreach (var diretorio in diretorios)
            {
                foreach (var tag in diretorio.Tags)
                {
                    quantidade++;

                    dataGridViewMetadados.Rows.Add(
                        diretorio.Name,
                        tag.Name,
                        $"0x{tag.Type:X4}",
                        tag.Description ?? "");
                }
            }

            if (quantidade == 0)
            {
                dataGridViewMetadados.Rows.Add(
                    "Informação",
                    "Metadados",
                    "-",
                    "Nenhum metadado encontrado.");
            }

            dataGridViewMetadados.AutoResizeRows(
                DataGridViewAutoSizeRowsMode.AllCells);
        }

        // ============================================================
        // REMOVER METADADOS
        // ============================================================

        private void btnRemoverMetadados_Click(
            object sender,
            EventArgs e)
        {
            if (string.IsNullOrEmpty(
                caminhoImagemOriginal))
            {
                MessageBox.Show(
                    "Selecione uma imagem primeiro.",
                    "Aviso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            try
            {
                byte[] dadosOriginais =
                    File.ReadAllBytes(
                        caminhoImagemOriginal);

                byte[] dadosLimpos;

                // ----------------------------------------------------
                // PNG
                // ----------------------------------------------------

                if (extensaoImagem == ".png")
                {
                    dadosLimpos =
                        RemoverMetadadosPng(
                            dadosOriginais);
                }

                // ----------------------------------------------------
                // JPEG
                // ----------------------------------------------------

                else if (extensaoImagem == ".jpg" ||
                         extensaoImagem == ".jpeg")
                {
                    dadosLimpos =
                        RemoverMetadadosJpeg(
                            dadosOriginais);
                }

                else
                {
                    MessageBox.Show(
                        "Este programa foi configurado para " +
                        "trabalhar com PNG e JPEG.",
                        "Formato não suportado",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                // ----------------------------------------------------
                // GUARDA A IMAGEM LIMPA
                // ----------------------------------------------------

                imagemSemMetadados =
                    dadosLimpos;

                // ----------------------------------------------------
                // MOSTRAR RESULTADO
                // ----------------------------------------------------

                MostrarImagemDosBytes(
                    imagemSemMetadados);

                // ----------------------------------------------------
                // MOSTRAR METADADOS RESTANTES
                // ----------------------------------------------------

                ExibirMetadadosDosBytes(
                    imagemSemMetadados);

                btnDownload.Enabled = true;

                MessageBox.Show(
                    "Os metadados foram removidos.\n\n" +
                    "Os dados da imagem não foram " +
                    "recomprimidos ou redesenhados.",
                    "Concluído",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Não foi possível remover os metadados.\n\n" +
                    ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ============================================================
        // PNG
        // ============================================================

        private byte[] RemoverMetadadosPng(
            byte[] dados)
        {
            byte[] assinatura =
            {
                0x89,
                0x50,
                0x4E,
                0x47,
                0x0D,
                0x0A,
                0x1A,
                0x0A
            };

            if (dados.Length < 8 ||
                !dados.Take(8).SequenceEqual(
                    assinatura))
            {
                throw new InvalidDataException(
                    "Arquivo PNG inválido.");
            }

            using (MemoryStream saida =
                   new MemoryStream())
            {
                // ----------------------------------------------------
                // ASSINATURA
                // ----------------------------------------------------

                saida.Write(
                    dados,
                    0,
                    8);

                int posicao = 8;

                while (posicao < dados.Length)
                {
                    if (posicao + 12 >
                        dados.Length)
                    {
                        throw new InvalidDataException(
                            "Chunk PNG inválido.");
                    }

                    uint tamanho =
                        LerUInt32BigEndian(
                            dados,
                            posicao);

                    int inicioChunk =
                        posicao;

                    int inicioTipo =
                        posicao + 4;

                    int tamanhoTotal =
                        checked(
                            12 +
                            (int)tamanho);

                    if (posicao +
                        tamanhoTotal >
                        dados.Length)
                    {
                        throw new InvalidDataException(
                            "Tamanho do chunk PNG inválido.");
                    }

                    string tipo =
                        Encoding.ASCII.GetString(
                            dados,
                            inicioTipo,
                            4);

                    bool remover =
                        DeveRemoverChunkPng(
                            tipo);

                    // ------------------------------------------------
                    // MANTER CHUNK
                    // ------------------------------------------------

                    if (!remover)
                    {
                        saida.Write(
                            dados,
                            inicioChunk,
                            tamanhoTotal);
                    }

                    posicao +=
                        tamanhoTotal;

                    if (tipo == "IEND")
                    {
                        break;
                    }
                }

                return saida.ToArray();
            }
        }

        // ============================================================
        // CHUNKS PNG QUE SERÃO REMOVIDOS
        // ============================================================

        private bool DeveRemoverChunkPng(
            string tipo)
        {
            // --------------------------------------------------------
            // TEXTO
            // --------------------------------------------------------

            if (tipo == "tEXt")
                return true;

            if (tipo == "zTXt")
                return true;

            if (tipo == "iTXt")
                return true;

            // --------------------------------------------------------
            // EXIF
            // --------------------------------------------------------

            if (tipo == "eXIf")
                return true;

            // --------------------------------------------------------
            // XMP
            // Normalmente armazenado dentro de iTXt.
            // --------------------------------------------------------

            // --------------------------------------------------------
            // PERFIL DE COR
            //
            // NÃO removemos iCCP.
            //
            // Ele é tecnicamente metadata, mas pode influenciar
            // a aparência das cores.
            // --------------------------------------------------------

            // --------------------------------------------------------
            // pHYs
            //
            // Também mantido porque informa resolução física.
            // Não altera os pixels.
            // --------------------------------------------------------

            return false;
        }

        // ============================================================
        // JPEG
        // ============================================================

        private byte[] RemoverMetadadosJpeg(
            byte[] dados)
        {
            if (dados == null ||
                dados.Length < 4)
            {
                throw new InvalidDataException(
                    "Arquivo JPEG inválido.");
            }

            if (dados[0] != 0xFF ||
                dados[1] != 0xD8)
            {
                throw new InvalidDataException(
                    "Arquivo JPEG inválido.");
            }

            using (MemoryStream saida =
                   new MemoryStream())
            {
                // ----------------------------------------------------
                // SOI
                // ----------------------------------------------------

                saida.WriteByte(0xFF);
                saida.WriteByte(0xD8);

                int posicao = 2;

                while (posicao < dados.Length)
                {
                    if (dados[posicao] != 0xFF)
                    {
                        saida.Write(
                            dados,
                            posicao,
                            dados.Length -
                            posicao);

                        break;
                    }

                    int inicioMarcador =
                        posicao;

                    while (posicao < dados.Length &&
                           dados[posicao] == 0xFF)
                    {
                        posicao++;
                    }

                    if (posicao >= dados.Length)
                        break;

                    byte marcador =
                        dados[posicao++];

                    // ------------------------------------------------
                    // SOS
                    // ------------------------------------------------

                    if (marcador == 0xDA)
                    {
                        int tamanho =
                            LerUInt16BigEndian(
                                dados,
                                posicao);

                        int fimCabecalho =
                            posicao +
                            tamanho;

                        if (fimCabecalho >
                            dados.Length)
                        {
                            throw new InvalidDataException(
                                "JPEG inválido.");
                        }

                        // Copia o SOS
                        saida.Write(
                            dados,
                            inicioMarcador,
                            fimCabecalho -
                            inicioMarcador);

                        // Copia os dados comprimidos
                        // sem modificá-los.
                        saida.Write(
                            dados,
                            fimCabecalho,
                            dados.Length -
                            fimCabecalho);

                        break;
                    }

                    // ------------------------------------------------
                    // MARCADORES SEM TAMANHO
                    // ------------------------------------------------

                    if (marcador == 0xD8 ||
                        marcador == 0xD9 ||
                        (marcador >= 0xD0 &&
                         marcador <= 0xD7))
                    {
                        saida.Write(
                            dados,
                            inicioMarcador,
                            posicao -
                            inicioMarcador);

                        continue;
                    }

                    if (posicao + 2 >
                        dados.Length)
                    {
                        throw new InvalidDataException(
                            "JPEG inválido.");
                    }

                    int tamanhoSegmento =
                        LerUInt16BigEndian(
                            dados,
                            posicao);

                    if (tamanhoSegmento < 2 ||
                        posicao +
                        tamanhoSegmento >
                        dados.Length)
                    {
                        throw new InvalidDataException(
                            "Segmento JPEG inválido.");
                    }

                    int fimSegmento =
                        posicao +
                        tamanhoSegmento;

                    bool remover =
                        DeveRemoverSegmentoJpeg(
                            marcador);

                    // ------------------------------------------------
                    // MANTER
                    // ------------------------------------------------

                    if (!remover)
                    {
                        saida.Write(
                            dados,
                            inicioMarcador,
                            fimSegmento -
                            inicioMarcador);
                    }

                    posicao =
                        fimSegmento;
                }

                return saida.ToArray();
            }
        }

        // ============================================================
        // SEGMENTOS JPEG QUE SERÃO REMOVIDOS
        // ============================================================

        private bool DeveRemoverSegmentoJpeg(
            byte marcador)
        {
            // --------------------------------------------------------
            // APP1
            //
            // EXIF
            // XMP
            // --------------------------------------------------------

            if (marcador == 0xE1)
                return true;

            // --------------------------------------------------------
            // APP13
            //
            // IPTC / Photoshop
            // --------------------------------------------------------

            if (marcador == 0xED)
                return true;

            // --------------------------------------------------------
            // COM
            //
            // Comentários
            // --------------------------------------------------------

            if (marcador == 0xFE)
                return true;

            // --------------------------------------------------------
            // APP2 = ICC Profile
            //
            // Mantemos para preservar a aparência das cores.
            // --------------------------------------------------------

            return false;
        }

        // ============================================================
        // MOSTRAR IMAGEM SEM METADADOS
        // ============================================================

        private void MostrarImagemDosBytes(
            byte[] dados)
        {
            using (MemoryStream memoria =
                   new MemoryStream(dados))
            {
                using (Image imagem =
                       Image.FromStream(memoria))
                {
                    if (pictureBox1.Image != null)
                    {
                        pictureBox1.Image.Dispose();
                        pictureBox1.Image = null;
                    }

                    pictureBox1.Image =
                        new Bitmap(imagem);
                }
            }
        }

        // ============================================================
        // VERIFICAR METADADOS APÓS A REMOÇÃO
        // ============================================================

        private void ExibirMetadadosDosBytes(
            byte[] dados)
        {
            dataGridViewMetadados.Rows.Clear();

            string arquivoTemporario =
                Path.Combine(
                    Path.GetTempPath(),
                    "RM_Metadados_" +
                    Guid.NewGuid().ToString("N") +
                    extensaoImagem);

            try
            {
                File.WriteAllBytes(
                    arquivoTemporario,
                    dados);

                var diretorios =
                    LerMetadados(
                        arquivoTemporario);

                int quantidade = 0;

                foreach (var diretorio in diretorios)
                {
                    foreach (var tag in diretorio.Tags)
                    {
                        quantidade++;

                        dataGridViewMetadados.Rows.Add(
                            diretorio.Name,
                            tag.Name,
                            $"0x{tag.Type:X4}",
                            tag.Description ?? "");
                    }
                }

                if (quantidade == 0)
                {
                    dataGridViewMetadados.Rows.Add(
                        "Resultado",
                        "Metadados",
                        "-",
                        "Nenhum metadado encontrado.");
                }

                dataGridViewMetadados.AutoResizeRows(
                    DataGridViewAutoSizeRowsMode.AllCells);
            }
            finally
            {
                try
                {
                    if (File.Exists(
                        arquivoTemporario))
                    {
                        File.Delete(
                            arquivoTemporario);
                    }
                }
                catch
                {
                }
            }
        }

        // ============================================================
        // EXPORTAR METADADOS
        // ============================================================

        private void btnDownloadMetadados_Click(
            object sender,
            EventArgs e)
        {
            if (string.IsNullOrEmpty(
                caminhoImagemOriginal))
            {
                MessageBox.Show(
                    "Selecione uma imagem primeiro.",
                    "Aviso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            try
            {
                var diretorios =
                    LerMetadados(
                        caminhoImagemOriginal);

                using (SaveFileDialog salvarMetadados =
                       new SaveFileDialog())
                {
                    salvarMetadados.Title =
                        "Exportar metadados";

                    salvarMetadados.Filter =
                        "Arquivo de texto|*.txt";

                    salvarMetadados.FileName =
                        Path.GetFileNameWithoutExtension(
                            caminhoImagemOriginal) +
                        "_metadados.txt";

                    if (salvarMetadados.ShowDialog() !=
                        DialogResult.OK)
                    {
                        return;
                    }

                    StringBuilder texto =
                        new StringBuilder();

                    texto.AppendLine(
                        "========================================");

                    texto.AppendLine(
                        "RM METADADOS");

                    texto.AppendLine(
                        "RELATÓRIO DE METADADOS DA IMAGEM");

                    texto.AppendLine(
                        "========================================");

                    texto.AppendLine();

                    texto.AppendLine(
                        "Arquivo: " +
                        Path.GetFileName(
                            caminhoImagemOriginal));

                    texto.AppendLine(
                        "Caminho: " +
                        caminhoImagemOriginal);

                    texto.AppendLine(
                        "Data da análise: " +
                        DateTime.Now.ToString(
                            "dd/MM/yyyy HH:mm:ss"));

                    texto.AppendLine();

                    foreach (var diretorio in diretorios)
                    {
                        texto.AppendLine(
                            "========================================");

                        texto.AppendLine(
                            diretorio.Name);

                        texto.AppendLine(
                            "========================================");

                        foreach (var tag in diretorio.Tags)
                        {
                            texto.AppendLine(
                                "Metadado: " +
                                tag.Name);

                            texto.AppendLine(
                                "ID: 0x" +
                                tag.Type.ToString("X4"));

                            texto.AppendLine(
                                "Valor: " +
                                (tag.Description ?? ""));

                            texto.AppendLine();
                        }
                    }

                    File.WriteAllText(
                        salvarMetadados.FileName,
                        texto.ToString(),
                        Encoding.UTF8);

                    MessageBox.Show(
                        "Metadados exportados com sucesso!",
                        "Concluído",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao exportar os metadados.\n\n" +
                    ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ============================================================
        // SALVAR IMAGEM LIMPA
        // ============================================================

        private void btnDownload_Click(
            object sender,
            EventArgs e)
        {
            if (imagemSemMetadados == null ||
                imagemSemMetadados.Length == 0)
            {
                MessageBox.Show(
                    "Primeiro remova os metadados da imagem.",
                    "Aviso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            using (SaveFileDialog salvarImagem =
                   new SaveFileDialog())
            {
                salvarImagem.Title =
                    "Salvar imagem sem metadados";

                if (extensaoImagem == ".png")
                {
                    salvarImagem.Filter =
                        "PNG|*.png";
                }
                else
                {
                    salvarImagem.Filter =
                        "JPEG|*.jpg;*.jpeg";
                }

                salvarImagem.FileName =
                    Path.GetFileNameWithoutExtension(
                        caminhoImagemOriginal) +
                    "_sem_metadados" +
                    extensaoImagem;

                if (salvarImagem.ShowDialog() !=
                    DialogResult.OK)
                {
                    return;
                }

                try
                {
                    // =================================================
                    // IMPORTANTE:
                    //
                    // Não usamos Image.Save().
                    //
                    // Gravamos os bytes diretamente.
                    //
                    // Portanto não existe uma nova compressão.
                    // =================================================

                    File.WriteAllBytes(
                        salvarImagem.FileName,
                        imagemSemMetadados);

                    MessageBox.Show(
                        "Imagem salva com sucesso!\n\n" +
                        "A imagem foi salva diretamente, " +
                        "sem nova compressão ou conversão.",
                        "Concluído",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Erro ao salvar a imagem.\n\n" +
                        ex.Message,
                        "Erro",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        // ============================================================
        // AUXILIAR JPEG
        // ============================================================

        private int LerUInt16BigEndian(
            byte[] dados,
            int posicao)
        {
            return
                (dados[posicao] << 8) |
                dados[posicao + 1];
        }

        // ============================================================
        // AUXILIAR PNG
        // ============================================================

        private uint LerUInt32BigEndian(
            byte[] dados,
            int posicao)
        {
            return
                ((uint)dados[posicao] << 24) |
                ((uint)dados[posicao + 1] << 16) |
                ((uint)dados[posicao + 2] << 8) |
                dados[posicao + 3];
        }

        // ============================================================
        // FECHAR PROGRAMA
        // ============================================================

        protected override void OnFormClosed(
            FormClosedEventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
            }

            base.OnFormClosed(e);
        }

        // ============================================================
        // LAYOUT
        // ============================================================

        private void ConfigurarLayoutFixo()
        {
            // --------------------------------------------------------
            // TÍTULO
            // --------------------------------------------------------

            label1.Location =
                new Point(
                    (this.ClientSize.Width -
                     label1.Width) / 2,
                    10);

            // --------------------------------------------------------
            // PICTUREBOX
            // --------------------------------------------------------

            pictureBox1.Location =
                new Point(20, 40);

            pictureBox1.Size =
                new Size(620, 280);

            pictureBox1.SizeMode =
                PictureBoxSizeMode.Zoom;

            pictureBox1.BorderStyle =
                BorderStyle.FixedSingle;

            // --------------------------------------------------------
            // BOTÕES
            // --------------------------------------------------------

            int yBotoes =
                pictureBox1.Bottom + 10;

            int larguraBotao = 145;
            int alturaBotao = 30;
            int espacamento = 5;

            btnSelecionar.Size =
                new Size(
                    larguraBotao,
                    alturaBotao);

            btnRemoverMetadados.Size =
                new Size(
                    larguraBotao,
                    alturaBotao);

            btnDownload.Size =
                new Size(
                    larguraBotao,
                    alturaBotao);

            btnDownloadMetadados.Size =
                new Size(
                    larguraBotao,
                    alturaBotao);

            int larguraTotal =
                (larguraBotao * 4) +
                (espacamento * 3);

            int xInicial =
                (this.ClientSize.Width -
                 larguraTotal) / 2;

            btnSelecionar.Location =
                new Point(
                    xInicial,
                    yBotoes);

            btnRemoverMetadados.Location =
                new Point(
                    xInicial +
                    larguraBotao +
                    espacamento,
                    yBotoes);

            btnDownload.Location =
                new Point(
                    xInicial +
                    (larguraBotao +
                     espacamento) * 2,
                    yBotoes);

            btnDownloadMetadados.Location =
                new Point(
                    xInicial +
                    (larguraBotao +
                     espacamento) * 3,
                    yBotoes);

            // --------------------------------------------------------
            // TABELA
            // --------------------------------------------------------

            int yTabela =
                yBotoes +
                alturaBotao +
                10;

            dataGridViewMetadados.Location =
                new Point(
                    20,
                    yTabela);

            dataGridViewMetadados.Size =
                new Size(
                    620,
                    this.ClientSize.Height -
                    yTabela -
                    20);
        }
    }
}

//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Drawing.Imaging;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Windows.Forms;
//using MetadataExtractor;

//namespace RM_Metadados
//{
//    public partial class Form1 : Form
//    {
//        private string caminhoImagemOriginal = null;

//        // Bytes da imagem depois da remoção dos metadados.
//        // Mantemos os bytes para não reconstruir os pixels.
//        private byte[] imagemSemMetadados = null;

//        // Formato da imagem processada.
//        private string extensaoImagemProcessada = null;

//        public Form1()
//        {
//            InitializeComponent();

//            // ============================================================
//            // JANELA FIXA
//            // ============================================================

//            this.FormBorderStyle = FormBorderStyle.FixedSingle;
//            this.MaximizeBox = false;
//            this.MinimizeBox = true;

//            this.ClientSize = new Size(660, 600);

//            // ============================================================
//            // BOTÕES
//            // ============================================================

//            btnRemoverMetadados.Enabled = false;
//            btnDownload.Enabled = false;
//            btnDownloadMetadados.Enabled = false;

//            // ============================================================
//            // TABELA
//            // ============================================================

//            ConfigurarTabelaMetadados();

//            // ============================================================
//            // LAYOUT
//            // ============================================================

//            ConfigurarLayoutFixo();
//        }

//        // ============================================================
//        // CONFIGURAÇÃO DA TABELA
//        // ============================================================

//        private void ConfigurarTabelaMetadados()
//        {
//            dataGridViewMetadados.Columns.Clear();
//            dataGridViewMetadados.Rows.Clear();

//            dataGridViewMetadados.Columns.Add(
//                "Grupo",
//                "Grupo");

//            dataGridViewMetadados.Columns.Add(
//                "Metadado",
//                "Metadado");

//            dataGridViewMetadados.Columns.Add(
//                "ID",
//                "ID");

//            dataGridViewMetadados.Columns.Add(
//                "Valor",
//                "Valor");

//            dataGridViewMetadados.ReadOnly = true;

//            dataGridViewMetadados.AllowUserToAddRows = false;
//            dataGridViewMetadados.AllowUserToDeleteRows = false;

//            dataGridViewMetadados.RowHeadersVisible = false;

//            dataGridViewMetadados.SelectionMode =
//                DataGridViewSelectionMode.FullRowSelect;

//            dataGridViewMetadados.MultiSelect = false;

//            dataGridViewMetadados.AutoSizeRowsMode =
//                DataGridViewAutoSizeRowsMode.AllCells;

//            dataGridViewMetadados.DefaultCellStyle.WrapMode =
//                DataGridViewTriState.True;

//            dataGridViewMetadados.Columns["Grupo"].Width = 125;

//            dataGridViewMetadados.Columns["Metadado"].Width = 165;

//            dataGridViewMetadados.Columns["ID"].Width = 65;

//            dataGridViewMetadados.Columns["Valor"].AutoSizeMode =
//                DataGridViewAutoSizeColumnMode.Fill;
//        }

//        // ============================================================
//        // SELECIONAR IMAGEM
//        // ============================================================

//        private void btnSelecionar_Click(object sender, EventArgs e)
//        {
//            using (OpenFileDialog abrirImagem =
//                   new OpenFileDialog())
//            {
//                abrirImagem.Title =
//                    "Selecionar imagem";

//                abrirImagem.Filter =
//                    "Imagens|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tiff;*.tif|" +
//                    "JPEG|*.jpg;*.jpeg|" +
//                    "PNG|*.png|" +
//                    "BMP|*.bmp|" +
//                    "GIF|*.gif|" +
//                    "TIFF|*.tif;*.tiff";

//                if (abrirImagem.ShowDialog() !=
//                    DialogResult.OK)
//                {
//                    return;
//                }

//                try
//                {
//                    // ==================================================
//                    // NOVO ARQUIVO
//                    // ==================================================

//                    caminhoImagemOriginal =
//                        abrirImagem.FileName;

//                    imagemSemMetadados = null;
//                    extensaoImagemProcessada = null;

//                    // ==================================================
//                    // LIBERA IMAGEM ANTERIOR DO PICTUREBOX
//                    // ==================================================

//                    if (pictureBox1.Image != null)
//                    {
//                        pictureBox1.Image.Dispose();
//                        pictureBox1.Image = null;
//                    }

//                    // ==================================================
//                    // LIMPA TABELA
//                    // ==================================================

//                    dataGridViewMetadados.Rows.Clear();

//                    // ==================================================
//                    // CARREGA A IMAGEM
//                    // ==================================================

//                    using (Image imagemOriginal =
//                           Image.FromFile(caminhoImagemOriginal))
//                    {
//                        pictureBox1.Image =
//                            new Bitmap(imagemOriginal);
//                    }

//                    // ==================================================
//                    // MOSTRA METADADOS ORIGINAIS
//                    // ==================================================

//                    ExibirMetadados(
//                        caminhoImagemOriginal);

//                    // ==================================================
//                    // BOTÕES
//                    // ==================================================

//                    btnRemoverMetadados.Enabled = true;

//                    btnDownloadMetadados.Enabled = true;

//                    btnDownload.Enabled = false;
//                }
//                catch (Exception ex)
//                {
//                    MessageBox.Show(
//                        "Não foi possível abrir a imagem.\n\n" +
//                        ex.Message,
//                        "Erro",
//                        MessageBoxButtons.OK,
//                        MessageBoxIcon.Error);
//                }
//            }
//        }

//        // ============================================================
//        // LER METADADOS COM METADATAEXTRACTOR
//        // ============================================================

//        private IReadOnlyList<MetadataExtractor.Directory>
//            LerMetadados(string caminho)
//        {
//            return ImageMetadataReader.ReadMetadata(caminho);
//        }

//        // ============================================================
//        // EXIBIR METADADOS
//        // ============================================================

//        private void ExibirMetadados(string caminho)
//        {
//            dataGridViewMetadados.Rows.Clear();

//            var diretorios =
//                LerMetadados(caminho);

//            int quantidade = 0;

//            foreach (var diretorio in diretorios)
//            {
//                foreach (var tag in diretorio.Tags)
//                {
//                    quantidade++;

//                    string grupo =
//                        diretorio.Name;

//                    string nome =
//                        tag.Name;

//                    string id =
//                        $"0x{tag.Type:X4}";

//                    string valor =
//                        tag.Description ?? "";

//                    dataGridViewMetadados.Rows.Add(
//                        grupo,
//                        nome,
//                        id,
//                        valor);
//                }
//            }

//            if (quantidade == 0)
//            {
//                dataGridViewMetadados.Rows.Add(
//                    "Informação",
//                    "Metadados",
//                    "-",
//                    "Nenhum metadado encontrado.");
//            }

//            dataGridViewMetadados.AutoResizeRows(
//                DataGridViewAutoSizeRowsMode.AllCells);
//        }

//        // ============================================================
//        // EXPORTAR METADADOS
//        // ============================================================

//        private void btnDownloadMetadados_Click(
//            object sender,
//            EventArgs e)
//        {
//            if (string.IsNullOrEmpty(
//                caminhoImagemOriginal))
//            {
//                MessageBox.Show(
//                    "Selecione uma imagem primeiro.",
//                    "Aviso",
//                    MessageBoxButtons.OK,
//                    MessageBoxIcon.Warning);

//                return;
//            }

//            try
//            {
//                var diretorios =
//                    LerMetadados(
//                        caminhoImagemOriginal);

//                using (SaveFileDialog salvarMetadados =
//                       new SaveFileDialog())
//                {
//                    salvarMetadados.Title =
//                        "Exportar metadados";

//                    salvarMetadados.Filter =
//                        "Arquivo de texto|*.txt";

//                    salvarMetadados.FileName =
//                        Path.GetFileNameWithoutExtension(
//                            caminhoImagemOriginal) +
//                        "_metadados.txt";

//                    if (salvarMetadados.ShowDialog() !=
//                        DialogResult.OK)
//                    {
//                        return;
//                    }

//                    StringBuilder texto =
//                        new StringBuilder();

//                    texto.AppendLine(
//                        "========================================");

//                    texto.AppendLine(
//                        "RM METADADOS");

//                    texto.AppendLine(
//                        "RELATÓRIO DE METADADOS DA IMAGEM");

//                    texto.AppendLine(
//                        "========================================");

//                    texto.AppendLine();

//                    texto.AppendLine(
//                        "Arquivo: " +
//                        Path.GetFileName(
//                            caminhoImagemOriginal));

//                    texto.AppendLine(
//                        "Caminho: " +
//                        caminhoImagemOriginal);

//                    texto.AppendLine(
//                        "Data da análise: " +
//                        DateTime.Now.ToString(
//                            "dd/MM/yyyy HH:mm:ss"));

//                    texto.AppendLine();

//                    foreach (var diretorio in diretorios)
//                    {
//                        texto.AppendLine(
//                            "========================================");

//                        texto.AppendLine(
//                            diretorio.Name);

//                        texto.AppendLine(
//                            "========================================");

//                        foreach (var tag in diretorio.Tags)
//                        {
//                            texto.AppendLine(
//                                "Metadado: " +
//                                tag.Name);

//                            texto.AppendLine(
//                                "ID: 0x" +
//                                tag.Type.ToString("X4"));

//                            texto.AppendLine(
//                                "Valor: " +
//                                (tag.Description ?? ""));

//                            texto.AppendLine();
//                        }
//                    }

//                    File.WriteAllText(
//                        salvarMetadados.FileName,
//                        texto.ToString(),
//                        Encoding.UTF8);

//                    MessageBox.Show(
//                        "Metadados exportados com sucesso!",
//                        "Concluído",
//                        MessageBoxButtons.OK,
//                        MessageBoxIcon.Information);
//                }
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show(
//                    "Erro ao exportar os metadados.\n\n" +
//                    ex.Message,
//                    "Erro",
//                    MessageBoxButtons.OK,
//                    MessageBoxIcon.Error);
//            }
//        }

//        // ============================================================
//        // REMOVER METADADOS
//        // ============================================================

//        private void btnRemoverMetadados_Click(
//            object sender,
//            EventArgs e)
//        {
//            if (string.IsNullOrEmpty(
//                caminhoImagemOriginal))
//            {
//                MessageBox.Show(
//                    "Selecione uma imagem primeiro.",
//                    "Aviso",
//                    MessageBoxButtons.OK,
//                    MessageBoxIcon.Warning);

//                return;
//            }

//            try
//            {
//                string extensao =
//                    Path.GetExtension(
//                        caminhoImagemOriginal)
//                        .ToLowerInvariant();

//                byte[] dadosOriginais =
//                    File.ReadAllBytes(
//                        caminhoImagemOriginal);

//                byte[] dadosLimpos;

//                // ======================================================
//                // JPEG
//                // ======================================================

//                if (extensao == ".jpg" ||
//                    extensao == ".jpeg")
//                {
//                    dadosLimpos =
//                        RemoverMetadadosJpeg(
//                            dadosOriginais);
//                }

//                // ======================================================
//                // PNG
//                // ======================================================

//                else if (extensao == ".png")
//                {
//                    dadosLimpos =
//                        RemoverMetadadosPng(
//                            dadosOriginais);
//                }

//                // ======================================================
//                // OUTROS FORMATOS
//                // ======================================================

//                else
//                {
//                    MessageBox.Show(
//                        "Para preservar integralmente os dados da imagem " +
//                        "sem reconstruí-la, a remoção estrutural está " +
//                        "implementada atualmente para JPEG e PNG.\n\n" +
//                        "Este formato não será alterado.",
//                        "Formato não suportado para remoção segura",
//                        MessageBoxButtons.OK,
//                        MessageBoxIcon.Warning);

//                    return;
//                }

//                // ======================================================
//                // GUARDA OS BYTES DA IMAGEM LIMPA
//                // ======================================================

//                imagemSemMetadados =
//                    dadosLimpos;

//                extensaoImagemProcessada =
//                    extensao;

//                // ======================================================
//                // MOSTRA A IMAGEM LIMPA
//                // ======================================================

//                using (MemoryStream memoria =
//                       new MemoryStream(
//                           imagemSemMetadados))
//                {
//                    using (Image imagemLimpa =
//                           Image.FromStream(
//                               memoria))
//                    {
//                        if (pictureBox1.Image != null)
//                        {
//                            pictureBox1.Image.Dispose();
//                            pictureBox1.Image = null;
//                        }

//                        pictureBox1.Image =
//                            new Bitmap(imagemLimpa);
//                    }
//                }

//                // ======================================================
//                // MOSTRA O RESULTADO DOS METADADOS
//                // ======================================================

//                ExibirMetadadosDosBytes(
//                    imagemSemMetadados);

//                btnDownload.Enabled = true;

//                MessageBox.Show(
//                    "Os metadados removíveis foram eliminados sem " +
//                    "reconstruir os pixels da imagem.",
//                    "Concluído",
//                    MessageBoxButtons.OK,
//                    MessageBoxIcon.Information);
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show(
//                    "Não foi possível remover os metadados.\n\n" +
//                    ex.Message,
//                    "Erro",
//                    MessageBoxButtons.OK,
//                    MessageBoxIcon.Error);
//            }
//        }

//        // ============================================================
//        // REMOVER METADADOS JPEG
//        // ============================================================

//        private byte[] RemoverMetadadosJpeg(
//            byte[] dados)
//        {
//            if (dados == null ||
//                dados.Length < 4)
//            {
//                throw new InvalidDataException(
//                    "Arquivo JPEG inválido.");
//            }

//            if (dados[0] != 0xFF ||
//                dados[1] != 0xD8)
//            {
//                throw new InvalidDataException(
//                    "O arquivo não possui uma assinatura JPEG válida.");
//            }

//            using (MemoryStream saida =
//                   new MemoryStream())
//            {
//                // SOI
//                saida.WriteByte(0xFF);
//                saida.WriteByte(0xD8);

//                int posicao = 2;

//                while (posicao < dados.Length)
//                {
//                    // ==================================================
//                    // Verifica marcador
//                    // ==================================================

//                    if (dados[posicao] != 0xFF)
//                    {
//                        // Dados comprimidos.
//                        saida.Write(
//                            dados,
//                            posicao,
//                            dados.Length - posicao);

//                        break;
//                    }

//                    int inicioMarcador = posicao;

//                    while (posicao < dados.Length &&
//                           dados[posicao] == 0xFF)
//                    {
//                        posicao++;
//                    }

//                    if (posicao >= dados.Length)
//                        break;

//                    byte marcador =
//                        dados[posicao++];

//                    // ==================================================
//                    // SOS - início dos dados da imagem
//                    // ==================================================

//                    if (marcador == 0xDA)
//                    {
//                        int tamanho =
//                            LerUInt16BigEndian(
//                                dados,
//                                posicao);

//                        int fimCabecalho =
//                            posicao + tamanho;

//                        if (fimCabecalho >
//                            dados.Length)
//                        {
//                            throw new InvalidDataException(
//                                "JPEG inválido.");
//                        }

//                        // Copia o cabeçalho SOS
//                        saida.Write(
//                            dados,
//                            inicioMarcador,
//                            fimCabecalho -
//                            inicioMarcador);

//                        // Copia todo o restante dos dados comprimidos.
//                        saida.Write(
//                            dados,
//                            fimCabecalho,
//                            dados.Length -
//                            fimCabecalho);

//                        break;
//                    }

//                    // ==================================================
//                    // Marcadores sem tamanho
//                    // ==================================================

//                    if (marcador == 0xD8 ||
//                        marcador == 0xD9 ||
//                        (marcador >= 0xD0 &&
//                         marcador <= 0xD7))
//                    {
//                        saida.Write(
//                            dados,
//                            inicioMarcador,
//                            posicao -
//                            inicioMarcador);

//                        continue;
//                    }

//                    // ==================================================
//                    // Lê tamanho do segmento
//                    // ==================================================

//                    if (posicao + 2 >
//                        dados.Length)
//                    {
//                        throw new InvalidDataException(
//                            "Segmento JPEG inválido.");
//                    }

//                    int tamanhoSegmento =
//                        LerUInt16BigEndian(
//                            dados,
//                            posicao);

//                    if (tamanhoSegmento < 2 ||
//                        posicao +
//                        tamanhoSegmento >
//                        dados.Length)
//                    {
//                        throw new InvalidDataException(
//                            "Tamanho de segmento JPEG inválido.");
//                    }

//                    int fimSegmento =
//                        posicao +
//                        tamanhoSegmento;

//                    bool remover =
//                        DeveRemoverSegmentoJpeg(
//                            marcador,
//                            dados,
//                            posicao,
//                            tamanhoSegmento);

//                    // ==================================================
//                    // Mantém o segmento
//                    // ==================================================

//                    if (!remover)
//                    {
//                        saida.Write(
//                            dados,
//                            inicioMarcador,
//                            fimSegmento -
//                            inicioMarcador);
//                    }

//                    posicao =
//                        fimSegmento;
//                }

//                return saida.ToArray();
//            }
//        }

//        // ============================================================
//        // DECIDE QUAIS SEGMENTOS JPEG SÃO METADADOS
//        // ============================================================

//        private bool DeveRemoverSegmentoJpeg(
//            byte marcador,
//            byte[] dados,
//            int posicaoTamanho,
//            int tamanhoSegmento)
//        {
//            // APP1
//            // EXIF / XMP
//            if (marcador == 0xE1)
//            {
//                return true;
//            }

//            // APP13
//            // Photoshop / IPTC
//            if (marcador == 0xED)
//            {
//                return true;
//            }

//            // COM
//            // Comentário JPEG
//            if (marcador == 0xFE)
//            {
//                return true;
//            }

//            // APP0 = JFIF
//            // Mantemos porque é estrutura do JPEG.

//            // APP2 pode conter ICC Profile.
//            // Mantemos para preservar as cores.
//            if (marcador == 0xE2)
//            {
//                return false;
//            }

//            return false;
//        }

//        // ============================================================
//        // REMOVER METADADOS PNG
//        // ============================================================

//        private byte[] RemoverMetadadosPng(
//            byte[] dados)
//        {
//            byte[] assinatura =
//            {
//                0x89,
//                0x50,
//                0x4E,
//                0x47,
//                0x0D,
//                0x0A,
//                0x1A,
//                0x0A
//            };

//            if (dados.Length < 8 ||
//                !dados.Take(8).SequenceEqual(
//                    assinatura))
//            {
//                throw new InvalidDataException(
//                    "O arquivo não possui uma assinatura PNG válida.");
//            }

//            using (MemoryStream saida =
//                   new MemoryStream())
//            {
//                // Copia assinatura
//                saida.Write(
//                    dados,
//                    0,
//                    8);

//                int posicao = 8;

//                while (posicao < dados.Length)
//                {
//                    if (posicao + 12 >
//                        dados.Length)
//                    {
//                        throw new InvalidDataException(
//                            "Chunk PNG inválido.");
//                    }

//                    uint tamanho =
//                        LerUInt32BigEndian(
//                            dados,
//                            posicao);

//                    int inicioChunk =
//                        posicao;

//                    int inicioTipo =
//                        posicao + 4;

//                    int tamanhoTotal =
//                        checked(
//                            12 +
//                            (int)tamanho);

//                    if (posicao +
//                        tamanhoTotal >
//                        dados.Length)
//                    {
//                        throw new InvalidDataException(
//                            "Tamanho de chunk PNG inválido.");
//                    }

//                    string tipo =
//                        Encoding.ASCII.GetString(
//                            dados,
//                            inicioTipo,
//                            4);

//                    bool remover =
//                        DeveRemoverChunkPng(
//                            tipo);

//                    // ==================================================
//                    // Mantém o chunk
//                    // ==================================================

//                    if (!remover)
//                    {
//                        saida.Write(
//                            dados,
//                            inicioChunk,
//                            tamanhoTotal);
//                    }

//                    posicao +=
//                        tamanhoTotal;

//                    // IEND é o último chunk.
//                    if (tipo == "IEND")
//                    {
//                        break;
//                    }
//                }

//                return saida.ToArray();
//            }
//        }

//        // ============================================================
//        // DECIDE QUAIS CHUNKS PNG SÃO METADADOS
//        // ============================================================

//        private bool DeveRemoverChunkPng(
//            string tipo)
//        {
//            // --------------------------------------------------------
//            // Metadados textuais
//            // --------------------------------------------------------

//            if (tipo == "tEXt")
//                return true;

//            if (tipo == "zTXt")
//                return true;

//            if (tipo == "iTXt")
//                return true;

//            // --------------------------------------------------------
//            // EXIF
//            // --------------------------------------------------------

//            if (tipo == "eXIf")
//                return true;

//            // --------------------------------------------------------
//            // XMP / XML
//            // --------------------------------------------------------

//            if (tipo == "iTXt")
//                return true;

//            // --------------------------------------------------------
//            // Mantemos:
//            //
//            // iCCP = perfil de cor
//            // pHYs = resolução física
//            //
//            // porque removê-los pode alterar a interpretação
//            // da imagem em determinados programas.
//            // --------------------------------------------------------

//            return false;
//        }

//        // ============================================================
//        // VERIFICAR METADADOS DOS BYTES PROCESSADOS
//        // ============================================================

//        private void ExibirMetadadosDosBytes(
//            byte[] dados)
//        {
//            dataGridViewMetadados.Rows.Clear();

//            if (dados == null ||
//                dados.Length == 0)
//            {
//                return;
//            }

//            string arquivoTemporario =
//                Path.Combine(
//                    Path.GetTempPath(),
//                    "RM_Metadados_" +
//                    Guid.NewGuid().ToString("N") +
//                    extensaoImagemProcessada);

//            try
//            {
//                File.WriteAllBytes(
//                    arquivoTemporario,
//                    dados);

//                var diretorios =
//                    LerMetadados(
//                        arquivoTemporario);

//                int quantidade = 0;

//                foreach (var diretorio in diretorios)
//                {
//                    foreach (var tag in diretorio.Tags)
//                    {
//                        quantidade++;

//                        dataGridViewMetadados.Rows.Add(
//                            diretorio.Name,
//                            tag.Name,
//                            $"0x{tag.Type:X4}",
//                            tag.Description ?? "");
//                    }
//                }

//                if (quantidade == 0)
//                {
//                    dataGridViewMetadados.Rows.Add(
//                        "Resultado",
//                        "Metadados",
//                        "-",
//                        "Nenhum metadado encontrado.");
//                }

//                dataGridViewMetadados.AutoResizeRows(
//                    DataGridViewAutoSizeRowsMode.AllCells);
//            }
//            finally
//            {
//                try
//                {
//                    if (File.Exists(
//                        arquivoTemporario))
//                    {
//                        File.Delete(
//                            arquivoTemporario);
//                    }
//                }
//                catch
//                {
//                    // Ignora erro ao apagar temporário.
//                }
//            }
//        }

//        // ============================================================
//        // SALVAR IMAGEM SEM METADADOS
//        // ============================================================

//        private void btnDownload_Click(
//            object sender,
//            EventArgs e)
//        {
//            if (imagemSemMetadados == null ||
//                imagemSemMetadados.Length == 0)
//            {
//                MessageBox.Show(
//                    "Primeiro remova os metadados da imagem.",
//                    "Aviso",
//                    MessageBoxButtons.OK,
//                    MessageBoxIcon.Warning);

//                return;
//            }

//            using (SaveFileDialog salvarImagem =
//                   new SaveFileDialog())
//            {
//                salvarImagem.Title =
//                    "Salvar imagem sem metadados";

//                // Mantém o mesmo formato da imagem original.
//                if (extensaoImagemProcessada == ".jpg" ||
//                    extensaoImagemProcessada == ".jpeg")
//                {
//                    salvarImagem.Filter =
//                        "JPEG|*.jpg;*.jpeg";
//                }
//                else if (extensaoImagemProcessada == ".png")
//                {
//                    salvarImagem.Filter =
//                        "PNG|*.png";
//                }
//                else
//                {
//                    salvarImagem.Filter =
//                        "Imagem|*" +
//                        extensaoImagemProcessada;
//                }

//                salvarImagem.FileName =
//                    Path.GetFileNameWithoutExtension(
//                        caminhoImagemOriginal) +
//                    "_sem_metadados" +
//                    extensaoImagemProcessada;

//                if (salvarImagem.ShowDialog() !=
//                    DialogResult.OK)
//                {
//                    return;
//                }

//                try
//                {
//                    File.WriteAllBytes(
//                        salvarImagem.FileName,
//                        imagemSemMetadados);

//                    MessageBox.Show(
//                        "Imagem salva com sucesso!\n\n" +
//                        "Os dados da imagem foram preservados " +
//                        "sem uma nova codificação.",
//                        "Concluído",
//                        MessageBoxButtons.OK,
//                        MessageBoxIcon.Information);
//                }
//                catch (Exception ex)
//                {
//                    MessageBox.Show(
//                        "Erro ao salvar a imagem.\n\n" +
//                        ex.Message,
//                        "Erro",
//                        MessageBoxButtons.OK,
//                        MessageBoxIcon.Error);
//                }
//            }
//        }

//        // ============================================================
//        // FUNÇÕES AUXILIARES - JPEG
//        // ============================================================

//        private int LerUInt16BigEndian(
//            byte[] dados,
//            int posicao)
//        {
//            return
//                (dados[posicao] << 8) |
//                dados[posicao + 1];
//        }

//        // ============================================================
//        // FUNÇÃO AUXILIAR - PNG
//        // ============================================================

//        private uint LerUInt32BigEndian(
//            byte[] dados,
//            int posicao)
//        {
//            return
//                ((uint)dados[posicao] << 24) |
//                ((uint)dados[posicao + 1] << 16) |
//                ((uint)dados[posicao + 2] << 8) |
//                dados[posicao + 3];
//        }

//        // ============================================================
//        // LIBERAR RECURSOS
//        // ============================================================

//        protected override void OnFormClosed(
//            FormClosedEventArgs e)
//        {
//            if (pictureBox1.Image != null)
//            {
//                pictureBox1.Image.Dispose();
//            }

//            base.OnFormClosed(e);
//        }

//        // ============================================================
//        // LAYOUT FIXO
//        // ============================================================

//        private void ConfigurarLayoutFixo()
//        {
//            // --------------------------------------------------------
//            // TÍTULO
//            // --------------------------------------------------------

//            label1.Location =
//                new Point(
//                    (this.ClientSize.Width -
//                     label1.Width) / 2,
//                    10);

//            // --------------------------------------------------------
//            // IMAGEM
//            // --------------------------------------------------------

//            pictureBox1.Location =
//                new Point(20, 40);

//            pictureBox1.Size =
//                new Size(620, 280);

//            pictureBox1.SizeMode =
//                PictureBoxSizeMode.Zoom;

//            pictureBox1.BorderStyle =
//                BorderStyle.FixedSingle;

//            // --------------------------------------------------------
//            // BOTÕES
//            // --------------------------------------------------------

//            int yBotoes =
//                pictureBox1.Bottom + 10;

//            int larguraBotao = 145;
//            int alturaBotao = 30;
//            int espacamento = 5;

//            btnSelecionar.Size =
//                new Size(
//                    larguraBotao,
//                    alturaBotao);

//            btnRemoverMetadados.Size =
//                new Size(
//                    larguraBotao,
//                    alturaBotao);

//            btnDownload.Size =
//                new Size(
//                    larguraBotao,
//                    alturaBotao);

//            btnDownloadMetadados.Size =
//                new Size(
//                    larguraBotao,
//                    alturaBotao);

//            int larguraTotal =
//                (larguraBotao * 4) +
//                (espacamento * 3);

//            int xInicial =
//                (this.ClientSize.Width -
//                 larguraTotal) / 2;

//            btnSelecionar.Location =
//                new Point(
//                    xInicial,
//                    yBotoes);

//            btnRemoverMetadados.Location =
//                new Point(
//                    xInicial +
//                    larguraBotao +
//                    espacamento,
//                    yBotoes);

//            btnDownload.Location =
//                new Point(
//                    xInicial +
//                    (larguraBotao +
//                     espacamento) * 2,
//                    yBotoes);

//            btnDownloadMetadados.Location =
//                new Point(
//                    xInicial +
//                    (larguraBotao +
//                     espacamento) * 3,
//                    yBotoes);

//            // --------------------------------------------------------
//            // TABELA
//            // --------------------------------------------------------

//            int yTabela =
//                yBotoes +
//                alturaBotao +
//                10;

//            dataGridViewMetadados.Location =
//                new Point(
//                    20,
//                    yTabela);

//            dataGridViewMetadados.Size =
//                new Size(
//                    620,
//                    this.ClientSize.Height -
//                    yTabela -
//                    20);
//        }
//    }
//}

////using System;
////using System.Drawing;
////using System.Drawing.Imaging;
////using System.IO;
////using System.Linq;
////using System.Text;
////using System.Windows.Forms;
////using MetadataExtractor;
////using MetadataExtractor.Formats.Exif;
////using MetadataExtractor.Formats.Iptc;
////using MetadataExtractor.Formats.Xmp;

////namespace RM_Metadados
////{
////    public partial class Form1 : Form
////    {
////        private string caminhoImagemOriginal = null;
////        private Bitmap imagemSemMetadados = null;

////        public Form1()
////        {
////            InitializeComponent();

////            // Janela com tamanho fixo
////            this.FormBorderStyle = FormBorderStyle.FixedSingle;
////            this.MaximizeBox = false;
////            this.MinimizeBox = true;

////            // Tamanho fixo da janela
////            this.ClientSize = new Size(660, 600);//this.ClientSize = new Size(760, 700);

////            btnRemoverMetadados.Enabled = false;
////            btnDownload.Enabled = false;
////            btnDownloadMetadados.Enabled = false;

////            ConfigurarTabelaMetadados();

////            ConfigurarLayoutFixo();
////        }

////        // ============================================================
////        // CONFIGURAÇÃO DA TABELA
////        // ============================================================

////        private void ConfigurarTabelaMetadados()
////        {
////            dataGridViewMetadados.Columns.Clear();
////            dataGridViewMetadados.Rows.Clear();

////            dataGridViewMetadados.Columns.Add(
////                "Grupo",
////                "Grupo");

////            dataGridViewMetadados.Columns.Add(
////                "Metadado",
////                "Metadado");

////            dataGridViewMetadados.Columns.Add(
////                "Valor",
////                "Valor");

////            dataGridViewMetadados.ReadOnly = true;

////            dataGridViewMetadados.AllowUserToAddRows = false;
////            dataGridViewMetadados.AllowUserToDeleteRows = false;

////            dataGridViewMetadados.RowHeadersVisible = false;

////            dataGridViewMetadados.AutoSizeColumnsMode =
////                DataGridViewAutoSizeColumnsMode.Fill;

////            dataGridViewMetadados.SelectionMode =
////                DataGridViewSelectionMode.FullRowSelect;

////            dataGridViewMetadados.MultiSelect = false;
////        }

////        // ============================================================
////        // SELECIONAR IMAGEM
////        // ============================================================

////        private void btnSelecionar_Click(object sender, EventArgs e)
////        {
////            using (OpenFileDialog abrirImagem =
////                   new OpenFileDialog())
////            {
////                abrirImagem.Title =
////                    "Selecionar imagem";

////                abrirImagem.Filter =
////                    "Imagens|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tiff|" +
////                    "JPEG|*.jpg;*.jpeg|" +
////                    "PNG|*.png|" +
////                    "BMP|*.bmp|" +
////                    "GIF|*.gif|" +
////                    "TIFF|*.tiff";

////                if (abrirImagem.ShowDialog() !=
////                    DialogResult.OK)
////                {
////                    return;
////                }

////                try
////                {
////                    caminhoImagemOriginal =
////                        abrirImagem.FileName;

////                    // Libera imagem anterior
////                    if (pictureBox1.Image != null)
////                    {
////                        pictureBox1.Image.Dispose();
////                        pictureBox1.Image = null;
////                    }

////                    // Libera imagem sem metadados anterior
////                    if (imagemSemMetadados != null)
////                    {
////                        imagemSemMetadados.Dispose();
////                        imagemSemMetadados = null;
////                    }

////                    // Limpa tabela
////                    dataGridViewMetadados.Rows.Clear();

////                    using (Image imagemOriginal =
////                           Image.FromFile(
////                               caminhoImagemOriginal))
////                    {
////                        pictureBox1.Image =
////                            new Bitmap(imagemOriginal);

////                        // Lê e mostra os metadados
////                        ExibirMetadados(
////                            caminhoImagemOriginal);

////                        //// Ajusta janela
////                        //AjustarJanelaParaImagem(
////                        //    imagemOriginal);
////                    }

////                    // Habilita os botões
////                    btnRemoverMetadados.Enabled = true;

////                    btnDownloadMetadados.Enabled = true;

////                    // Ainda não existe imagem limpa
////                    btnDownload.Enabled = false;
////                }
////                catch (Exception ex)
////                {
////                    MessageBox.Show(
////                        "Não foi possível abrir a imagem.\n\n" +
////                        ex.Message,
////                        "Erro",
////                        MessageBoxButtons.OK,
////                        MessageBoxIcon.Error);
////                }
////            }
////        }

////        // ============================================================
////        // LER METADADOS COM METADATAEXTRACTOR
////        // ============================================================

////        private System.Collections.Generic.IReadOnlyList<
////            MetadataExtractor.Directory> LerMetadados(
////                string caminho)
////        {
////            return ImageMetadataReader.ReadMetadata(
////                caminho);
////        }

////        // ============================================================
////        // EXIBIR METADADOS NA TABELA
////        // ============================================================

////        private void ExibirMetadados(string caminho)
////        {
////            dataGridViewMetadados.Rows.Clear();

////            var diretorios = LerMetadados(caminho);

////            int quantidade = 0;

////            foreach (var diretorio in diretorios)
////            {
////                foreach (var tag in diretorio.Tags)
////                {
////                    quantidade++;

////                    string grupo =
////                        diretorio.Name;

////                    string nome =
////                        tag.Name;

////                    string valor =
////                        tag.Description ?? "";

////                    dataGridViewMetadados.Rows.Add(
////                        grupo,
////                        nome,
////                        valor);
////                }
////            }

////            if (quantidade == 0)
////            {
////                dataGridViewMetadados.Rows.Add(
////                    "Informação",
////                    "Metadados",
////                    "Nenhum metadado encontrado.");
////            }

////            dataGridViewMetadados.AutoResizeRows(
////                DataGridViewAutoSizeRowsMode.AllCells);
////        }

////        // ============================================================
////        // EXPORTAR METADADOS
////        // ============================================================

////        private void btnDownloadMetadados_Click(
////            object sender,
////            EventArgs e)
////        {
////            if (string.IsNullOrEmpty(
////                caminhoImagemOriginal))
////            {
////                MessageBox.Show(
////                    "Selecione uma imagem primeiro.",
////                    "Aviso",
////                    MessageBoxButtons.OK,
////                    MessageBoxIcon.Warning);

////                return;
////            }

////            try
////            {
////                var diretorios =
////                    LerMetadados(
////                        caminhoImagemOriginal);

////                using (SaveFileDialog salvarMetadados =
////                       new SaveFileDialog())
////                {
////                    salvarMetadados.Title =
////                        "Exportar metadados";

////                    salvarMetadados.Filter =
////                        "Arquivo de texto|*.txt";

////                    salvarMetadados.FileName =
////                        Path.GetFileNameWithoutExtension(
////                            caminhoImagemOriginal) +
////                        "_metadados.txt";

////                    if (salvarMetadados.ShowDialog() !=
////                        DialogResult.OK)
////                    {
////                        return;
////                    }

////                    StringBuilder texto =
////                        new StringBuilder();

////                    texto.AppendLine(
////                        "========================================");

////                    texto.AppendLine(
////                        "RM METADADOS");

////                    texto.AppendLine(
////                        "RELATÓRIO DE METADADOS DA IMAGEM");

////                    texto.AppendLine(
////                        "========================================");

////                    texto.AppendLine();

////                    texto.AppendLine(
////                        "Arquivo: " +
////                        Path.GetFileName(
////                            caminhoImagemOriginal));

////                    texto.AppendLine(
////                        "Caminho: " +
////                        caminhoImagemOriginal);

////                    texto.AppendLine(
////                        "Data da análise: " +
////                        DateTime.Now.ToString(
////                            "dd/MM/yyyy HH:mm:ss"));

////                    texto.AppendLine();

////                    foreach (var diretorio in diretorios)
////                    {
////                        texto.AppendLine(
////                            "========================================");

////                        texto.AppendLine(
////                            diretorio.Name);

////                        texto.AppendLine(
////                            "========================================");

////                        foreach (var tag in diretorio.Tags)
////                        {
////                            texto.AppendLine(
////                                tag.Name +
////                                ": " +
////                                (tag.Description ?? ""));

////                            texto.AppendLine();
////                        }
////                    }

////                    File.WriteAllText(
////                        salvarMetadados.FileName,
////                        texto.ToString(),
////                        Encoding.UTF8);

////                    MessageBox.Show(
////                        "Metadados exportados com sucesso!",
////                        "Concluído",
////                        MessageBoxButtons.OK,
////                        MessageBoxIcon.Information);
////                }
////            }
////            catch (Exception ex)
////            {
////                MessageBox.Show(
////                    "Erro ao exportar os metadados.\n\n" +
////                    ex.Message,
////                    "Erro",
////                    MessageBoxButtons.OK,
////                    MessageBoxIcon.Error);
////            }
////        }

////        // ============================================================
////        // REMOVER METADADOS
////        // ============================================================

////        private void btnRemoverMetadados_Click(
////            object sender,
////            EventArgs e)
////        {
////            if (string.IsNullOrEmpty(
////                caminhoImagemOriginal))
////            {
////                MessageBox.Show(
////                    "Selecione uma imagem primeiro.",
////                    "Aviso",
////                    MessageBoxButtons.OK,
////                    MessageBoxIcon.Warning);

////                return;
////            }

////            try
////            {
////                using (Image imagemOriginal =
////                       Image.FromFile(
////                           caminhoImagemOriginal))
////                {
////                    // Cria nova imagem contendo somente os pixels
////                    Bitmap novaImagem =
////                        new Bitmap(
////                            imagemOriginal.Width,
////                            imagemOriginal.Height,
////                            PixelFormat.Format24bppRgb);

////                    using (Graphics graphics =
////                           Graphics.FromImage(
////                               novaImagem))
////                    {
////                        graphics.Clear(Color.White);

////                        graphics.DrawImage(
////                            imagemOriginal,
////                            new Rectangle(
////                                0,
////                                0,
////                                imagemOriginal.Width,
////                                imagemOriginal.Height));
////                    }

////                    // Remove propriedades que eventualmente
////                    // tenham sido criadas na nova imagem
////                    foreach (
////                        PropertyItem propriedade
////                        in novaImagem.PropertyItems.ToList())
////                    {
////                        try
////                        {
////                            novaImagem.RemovePropertyItem(
////                                propriedade.Id);
////                        }
////                        catch
////                        {
////                            // Ignora propriedades não removíveis.
////                        }
////                    }

////                    // Libera imagem anterior
////                    if (imagemSemMetadados != null)
////                    {
////                        imagemSemMetadados.Dispose();
////                    }

////                    imagemSemMetadados =
////                        novaImagem;

////                    // Atualiza PictureBox
////                    if (pictureBox1.Image != null)
////                    {
////                        pictureBox1.Image.Dispose();
////                    }

////                    pictureBox1.Image =
////                        new Bitmap(
////                            imagemSemMetadados);

////                    // ==================================================
////                    // IMPORTANTE:
////                    // A tabela agora representa a imagem limpa.
////                    // ==================================================

////                    ExibirMetadadosDaImagem(
////                        imagemSemMetadados);

////                    btnDownload.Enabled = true;

////                    MessageBox.Show(
////                        "Os metadados foram removidos com sucesso.",
////                        "Concluído",
////                        MessageBoxButtons.OK,
////                        MessageBoxIcon.Information);
////                }
////            }
////            catch (Exception ex)
////            {
////                MessageBox.Show(
////                    "Não foi possível processar a imagem.\n\n" +
////                    ex.Message,
////                    "Erro",
////                    MessageBoxButtons.OK,
////                    MessageBoxIcon.Error);
////            }
////        }

////        // ============================================================
////        // VERIFICA METADADOS DA IMAGEM PROCESSADA
////        // ============================================================

////        private void ExibirMetadadosDaImagem(
////            Image imagem)
////        {
////            dataGridViewMetadados.Rows.Clear();

////            if (imagem == null)
////                return;

////            if (imagem.PropertyItems.Length == 0)
////            {
////                dataGridViewMetadados.Rows.Add(
////                    "Resultado",
////                    "Metadados",
////                    "Nenhum metadado encontrado.");
////            }
////            else
////            {
////                foreach (
////                    PropertyItem propriedade
////                    in imagem.PropertyItems)
////                {
////                    dataGridViewMetadados.Rows.Add(
////                        "Resultado",
////                        $"ID 0x{propriedade.Id:X4}",
////                        "Metadado ainda presente");
////                }
////            }

////            dataGridViewMetadados.AutoResizeRows(
////                DataGridViewAutoSizeRowsMode.AllCells);
////        }

////        // ============================================================
////        // BAIXAR IMAGEM SEM METADADOS
////        // ============================================================

////        private void btnDownload_Click(
////            object sender,
////            EventArgs e)
////        {
////            if (imagemSemMetadados == null)
////            {
////                MessageBox.Show(
////                    "Primeiro remova os metadados da imagem.",
////                    "Aviso",
////                    MessageBoxButtons.OK,
////                    MessageBoxIcon.Warning);

////                return;
////            }

////            using (SaveFileDialog salvarImagem =
////                   new SaveFileDialog())
////            {
////                salvarImagem.Title =
////                    "Salvar imagem sem metadados";

////                salvarImagem.Filter =
////                    "JPEG|*.jpg|" +
////                    "PNG|*.png|" +
////                    "BMP|*.bmp|" +
////                    "TIFF|*.tiff";

////                salvarImagem.FileName =
////                    "imagem_sem_metadados.jpg";

////                if (salvarImagem.ShowDialog() !=
////                    DialogResult.OK)
////                {
////                    return;
////                }

////                try
////                {
////                    string extensao =
////                        Path.GetExtension(
////                            salvarImagem.FileName)
////                            .ToLower();

////                    ImageFormat formato;

////                    switch (extensao)
////                    {
////                        case ".png":
////                            formato = ImageFormat.Png;
////                            break;

////                        case ".bmp":
////                            formato = ImageFormat.Bmp;
////                            break;

////                        case ".tiff":
////                        case ".tif":
////                            formato = ImageFormat.Tiff;
////                            break;

////                        default:
////                            formato = ImageFormat.Jpeg;
////                            break;
////                    }

////                    imagemSemMetadados.Save(
////                        salvarImagem.FileName,
////                        formato);

////                    MessageBox.Show(
////                        "Imagem salva com sucesso!",
////                        "Concluído",
////                        MessageBoxButtons.OK,
////                        MessageBoxIcon.Information);
////                }
////                catch (Exception ex)
////                {
////                    MessageBox.Show(
////                        "Erro ao salvar a imagem.\n\n" +
////                        ex.Message,
////                        "Erro",
////                        MessageBoxButtons.OK,
////                        MessageBoxIcon.Error);
////                }
////            }
////        }

////        // ============================================================
////        // LIBERAR RECURSOS
////        // ============================================================

////        protected override void OnFormClosed(
////            FormClosedEventArgs e)
////        {
////            if (pictureBox1.Image != null)
////            {
////                pictureBox1.Image.Dispose();
////            }

////            if (imagemSemMetadados != null)
////            {
////                imagemSemMetadados.Dispose();
////            }

////            base.OnFormClosed(e);
////        }

////        private void ConfigurarLayoutFixo()
////        {
////            // Título
////            label1.Location = new Point(
////                (this.ClientSize.Width - label1.Width) / 2,
////                10
////            );

////            // Imagem
////            pictureBox1.Location = new Point(20, 40);
////            pictureBox1.Size = new Size(620, 280);
////            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
////            pictureBox1.BorderStyle = BorderStyle.FixedSingle;

////            // ==============================
////            // 4 BOTÕES IGUAIS E CENTRALIZADOS
////            // ==============================

////            int yBotoes = pictureBox1.Bottom + 10;

////            int larguraBotao = 145;
////            int alturaBotao = 30;
////            int espacamento = 5;

////            btnSelecionar.Size = new Size(larguraBotao, alturaBotao);
////            btnRemoverMetadados.Size = new Size(larguraBotao, alturaBotao);
////            btnDownload.Size = new Size(larguraBotao, alturaBotao);
////            btnDownloadMetadados.Size = new Size(larguraBotao, alturaBotao);

////            // Largura total dos quatro botões + espaços
////            int larguraTotal =
////                (larguraBotao * 4) +
////                (espacamento * 3);

////            // Centraliza o conjunto
////            int xInicial =
////                (this.ClientSize.Width - larguraTotal) / 2;

////            btnSelecionar.Location =
////                new Point(xInicial, yBotoes);

////            btnRemoverMetadados.Location =
////                new Point(
////                    xInicial + larguraBotao + espacamento,
////                    yBotoes);

////            btnDownload.Location =
////                new Point(
////                    xInicial + (larguraBotao + espacamento) * 2,
////                    yBotoes);

////            btnDownloadMetadados.Location =
////                new Point(
////                    xInicial + (larguraBotao + espacamento) * 3,
////                    yBotoes);

////            // ==============================
////            // TABELA
////            // ==============================

////            int yTabela = yBotoes + alturaBotao + 10;

////            dataGridViewMetadados.Location =
////                new Point(20, yTabela);

////            dataGridViewMetadados.Size =
////                new Size(
////                    620,
////                    this.ClientSize.Height - yTabela - 20);
////        }
////    }
////}
