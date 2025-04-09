<template>
	<div class="main-container">
		<div class="editor-container editor-container_classic-editor" ref="editorContainerElement">
			<div class="editor-container__editor">
				<div ref="editorElement">
					<ckeditor 
            v-if="editor && config" 
            :modelValue="localValue" 
            :editor="editor" 
            :config="config" 
            @update:modelValue="updateDescription" 
            @ready="onEditorReady"
            @blur="syncToParent" />
				</div>
			</div>
		</div>
	</div>
</template>

<script setup>
import { ref, onMounted, computed, watch } from 'vue';
import { Ckeditor } from '@ckeditor/ckeditor5-vue';

const props = defineProps({
  modelValue: String,
  uid: String,
});

const emit = defineEmits(['update:modelValue']);

// 編輯器的數據
const localValue = ref(props.modelValue);
const uid = ref(props.uid);

console.log("uid: " + uid.value);

// CKEditor 實例
let editorInstance = null;

// 當 CKEditor 準備好時，將初始數據設置到 CKEditor
const onEditorReady = (editor) => {
  editorInstance = editor;
  editor.setData(localValue.value || ""); // 初始化 CKEditor 的數據
};

// 失焦時同步數據到父元件
const syncToParent = () => {
  if (editorInstance) {
    const currentValue = editorInstance.getData(); // 獲取當前編輯器內容
    if (currentValue !== localValue.value) {
      localValue.value = currentValue;
      emit('update:modelValue', currentValue); // 更新父元件
    }
  }
};

const updateDescription = (newValue) => {
  console.log(newValue);
};

// 監控 props.modelValue 的變化
watch(
  () => props.modelValue,
  (newVal) => {
    localValue.value = newVal;
    if (editorInstance) {
      editorInstance.setData(newVal || ""); // 更新 CKEditor 的數據
    }
    if (newVal == null) {
      localValue.value = ''; // 如果為 null 或 undefined，設空字串
    }
  },
  { immediate: true } // 確保初始化時同步
);
import {
	ClassicEditor,
	Alignment,
	AutoImage,
	AutoLink,
	Autosave,
	BlockQuote,
	Bold,
	Bookmark,
	Code,
	CodeBlock,
	Essentials,
	FontBackgroundColor,
	FontColor,
	FontFamily,
	FontSize,
	FullPage,
	GeneralHtmlSupport,
	Heading,
	Highlight,
	HorizontalLine,
	HtmlComment,
	HtmlEmbed,
	ImageBlock,
	ImageInsert,
	ImageInsertViaUrl,
	ImageToolbar,
	ImageUpload,
	Indent,
	IndentBlock,
	Italic,
	Link,
	Paragraph,
	RemoveFormat,
	ShowBlocks,
	SimpleUploadAdapter,
	SourceEditing,
	SpecialCharacters,
	Strikethrough,
	Subscript,
	Superscript,
	Table,
	TableCaption,
	TableCellProperties,
	TableColumnResize,
	TableProperties,
	TableToolbar,
	Underline
} from 'ckeditor5';

import translations from 'ckeditor5/translations/zh.js';

import 'ckeditor5/ckeditor5.css';

/**
 * Create a free account with a trial: https://portal.ckeditor.com/checkout?plan=free
 */
const LICENSE_KEY = 'GPL'; // or <YOUR_LICENSE_KEY>.

const isLayoutReady = ref(false);

const editor = ClassicEditor;

const config = computed(() => {
	if (!isLayoutReady.value) {
		return null;
	}

	return {
		simpleUpload: {
          uploadUrl: `${import.meta.env.VITE_API_ROOT_URL}/group-buy-page/upload-image?uid=asdfas2323-23423-234`,
		  withCredentials: true,
        },
		toolbar: {
			items: [
				'sourceEditing',
				'showBlocks',
				'|',
				'heading',
				'|',
				'fontSize',
				'fontFamily',
				'fontColor',
				'fontBackgroundColor',
				'|',
				'bold',
				'italic',
				'underline',
				'strikethrough',
				'subscript',
				'superscript',
				'code',
				'removeFormat',
				'|',
				'specialCharacters',
				'horizontalLine',
				'link',
				'bookmark',
				'insertImage',
				'insertTable',
				'highlight',
				'blockQuote',
				'codeBlock',
				'htmlEmbed',
				'|',
				'alignment',
				'|',
				'outdent',
				'indent'
			],
			shouldNotGroupWhenFull: true
		},
		plugins: [
			Alignment,
			AutoImage,
			AutoLink,
			Autosave,
			BlockQuote,
			Bold,
			Bookmark,
			Code,
			CodeBlock,
			Essentials,
			FontBackgroundColor,
			FontColor,
			FontFamily,
			FontSize,
			FullPage,
			GeneralHtmlSupport,
			Heading,
			Highlight,
			HorizontalLine,
			HtmlComment,
			HtmlEmbed,
			ImageBlock,
			ImageInsert,
			ImageInsertViaUrl,
			ImageToolbar,
			ImageUpload,
			Indent,
			IndentBlock,
			Italic,
			Link,
			Paragraph,
			RemoveFormat,
			ShowBlocks,
			SimpleUploadAdapter,
			SourceEditing,
			SpecialCharacters,
			Strikethrough,
			Subscript,
			Superscript,
			Table,
			TableCaption,
			TableCellProperties,
			TableColumnResize,
			TableProperties,
			TableToolbar,
			Underline
		],
		fontFamily: {
			supportAllValues: true
		},
		fontSize: {
			options: [10, 12, 14, 'default', 18, 20, 22],
			supportAllValues: true
		},
		heading: {
			options: [
				{
					model: 'paragraph',
					title: 'Paragraph',
					class: 'ck-heading_paragraph'
				},
				{
					model: 'heading1',
					view: 'h1',
					title: 'Heading 1',
					class: 'ck-heading_heading1'
				},
				{
					model: 'heading2',
					view: 'h2',
					title: 'Heading 2',
					class: 'ck-heading_heading2'
				},
				{
					model: 'heading3',
					view: 'h3',
					title: 'Heading 3',
					class: 'ck-heading_heading3'
				},
				{
					model: 'heading4',
					view: 'h4',
					title: 'Heading 4',
					class: 'ck-heading_heading4'
				},
				{
					model: 'heading5',
					view: 'h5',
					title: 'Heading 5',
					class: 'ck-heading_heading5'
				},
				{
					model: 'heading6',
					view: 'h6',
					title: 'Heading 6',
					class: 'ck-heading_heading6'
				}
			]
		},
		htmlSupport: {
			allow: [
				{
					name: /^.*$/,
					styles: true,
					attributes: true,
					classes: true
				}
			]
		},
		image: {
			toolbar: ['imageTextAlternative']
		},
		initialData:'',
		language: 'zh',
		licenseKey: LICENSE_KEY,
		link: {
			addTargetToExternalLinks: true,
			defaultProtocol: 'https://',
			decorators: {
				toggleDownloadable: {
					mode: 'manual',
					label: 'Downloadable',
					attributes: {
						download: 'file'
					}
				}
			}
		},
		placeholder: '請輸入...',
    ui: {
      viewportOffset: { top: 3000 } // 設置偏移
    },
		table: {
			contentToolbar: ['tableColumn', 'tableRow', 'mergeTableCells', 'tableProperties', 'tableCellProperties']
		},
		translations: [translations]
	};
});

onMounted(() => {
	isLayoutReady.value = true;
});
</script>


<style>
.ck-editor__editable {
  height: 400px;
}

.main-container {
	font-family: 'Lato';
	width: fit-content;
	margin-left: auto;
	margin-right: auto;
}
.ck-powered-by{
  display: none;
}

.ck-content {
	font-family: 'Lato';
	line-height: 1.6;
	word-break: break-word;
}

h1{
  font-size: 2em;
}
h2{
  font-size: 1.5em;
}
</style>